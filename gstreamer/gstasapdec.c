/*
 * gstasapdec.c - ASAP plugin for GStreamer
 *
 * Copyright (C) 2011-2012  Piotr Fusik
 *
 * This file is part of ASAP (Another Slight Atari Player),
 * see http://asap.sourceforge.net
 *
 * ASAP is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published
 * by the Free Software Foundation; either version 2 of the License,
 * or (at your option) any later version.
 *
 * ASAP is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ASAP; if not, write to the Free Software Foundation, Inc.,
 * 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */

/**
 * SECTION:element-asapdec
 *
 * This element decodes .sap files to raw audio.
 * .sap files are small Atari XL/XE programs that are executed on an emulated 6502
 * and a POKEY sound chip.
 *
 * <refsect2>
 * <title>Example launch line</title>
 * |[
 * gst-launch filesrc location=Foo.sap ! asapdec ! audioconvert ! alsasink
 * ]|
 * </refsect2>
 */

#ifdef HAVE_CONFIG_H
#include <config.h>
#endif

#include <string.h> /* memcpy */
#include <gst/gst.h>

#include "gstasapdec.h"

GST_DEBUG_CATEGORY_STATIC (gst_asap_dec_debug);
#define GST_CAT_DEFAULT gst_asap_dec_debug

enum
{
  PROP_0,
  PROP_TUNE
};

/* the capabilities of the inputs and outputs. */
static GstStaticPadTemplate sink_factory = GST_STATIC_PAD_TEMPLATE ("sink",
    GST_PAD_SINK,
    GST_PAD_ALWAYS,
    GST_STATIC_CAPS ("audio/x-sap")
    );

static GstStaticPadTemplate src_factory = GST_STATIC_PAD_TEMPLATE ("src",
    GST_PAD_SRC,
    GST_PAD_ALWAYS,
    GST_STATIC_CAPS ("audio/x-raw-int, "
        "endianness = (int) LITTLE_ENDIAN, "
        "signed = (boolean) true, "
        "width = (int) 16, "
        "depth = (int) 16, "
        "rate = (int) 44100, "
        "channels = (int) [ 1, 2 ]")
    );

GST_BOILERPLATE (GstAsapDec, gst_asap_dec, GstElement, GST_TYPE_ELEMENT);

static void
play_loop (GstPad * pad)
{
#define BUFFER_SIZE 4096
  GstAsapDec *asapdec = GST_ASAPDEC (gst_pad_get_parent (pad));
  GstBuffer *out = gst_buffer_new_and_alloc (BUFFER_SIZE);
  int position;
  gint64 time;
  gint64 time2;
  GstFlowReturn ret;
  gst_buffer_set_caps (out, GST_PAD_CAPS (pad));

  position = ASAP_GetBlocksPlayed (asapdec->asap);
  time = gst_util_uint64_scale_int (position, GST_SECOND, ASAP_SAMPLE_RATE);
  ASAP_Generate (asapdec->asap, GST_BUFFER_DATA (out), GST_BUFFER_SIZE (out), ASAPSampleFormat_S16_L_E);
  GST_BUFFER_OFFSET (out) = position;
  GST_BUFFER_TIMESTAMP (out) = time;
  position = ASAP_GetBlocksPlayed (asapdec->asap);
  time2 = gst_util_uint64_scale_int (position, GST_SECOND, ASAP_SAMPLE_RATE);
  GST_BUFFER_OFFSET_END (out) = position;
  GST_BUFFER_DURATION (out) = time2 - time;

  ret = gst_pad_push (asapdec->srcpad, out);
  switch (ret) {
  case GST_FLOW_OK:
    break;
  case GST_FLOW_UNEXPECTED:
    /* perform EOS logic, FIXME, segment seek? */
    gst_pad_push_event (pad, gst_event_new_eos ());
    gst_pad_pause_task (pad);
    break;
  default:
    if (ret < GST_FLOW_UNEXPECTED || ret == GST_FLOW_NOT_LINKED) {
      /* for fatal errors we post an error message */
      GST_ELEMENT_ERROR (asapdec, STREAM, FAILED, (NULL), ("streaming task paused"));
      gst_pad_push_event (pad, gst_event_new_eos ());
    }
    GST_INFO_OBJECT (asapdec, "pausing task, reason");
    gst_pad_pause_task (pad);
    break;
  }

  gst_object_unref (asapdec);
}

static gboolean
start_play_tune (GstAsapDec * asapdec)
{
  const ASAPInfo *info;
  int song;
  GstCaps *caps;

  if (!ASAP_Load (asapdec->asap, NULL, asapdec->module, asapdec->module_len)) {
    GST_ELEMENT_ERROR (asapdec, LIBRARY, INIT, ("Could not load tune"), ("Could not load tune"));
    return FALSE;
  }

  info = ASAP_GetInfo (asapdec->asap);
  song = asapdec->tune_number;
  if (song > 0)
    song--;
  else
    song = ASAPInfo_GetDefaultSong (info);
  if (!ASAP_PlaySong (asapdec->asap, song, -1)) {
    GST_ELEMENT_ERROR (asapdec, LIBRARY, INIT, ("Could not initialize song"), ("Could not initialize song"));
    return FALSE;
  }

  caps = gst_caps_new_simple ("audio/x-raw-int",
      "endianness", G_TYPE_INT, G_LITTLE_ENDIAN,
      "signed", G_TYPE_BOOLEAN, TRUE,
      "width", G_TYPE_INT, 16,
      "depth", G_TYPE_INT, 16,
      "rate", G_TYPE_INT, ASAP_SAMPLE_RATE,
      "channels", G_TYPE_INT, ASAPInfo_GetChannels (info), NULL);
  gst_pad_set_caps (asapdec->srcpad, caps);
  gst_caps_unref (caps);
  gst_pad_push_event (asapdec->srcpad, gst_event_new_new_segment (FALSE, 1.0, GST_FORMAT_TIME, 0, -1, 0));
  return gst_pad_start_task (asapdec->srcpad, (GstTaskFunction) play_loop, asapdec->srcpad);
}

static gboolean
gst_asap_dec_event (GstPad * pad, GstEvent * event)
{
  GstAsapDec *asapdec = GST_ASAPDEC (gst_pad_get_parent (pad));
  gboolean res;
  switch (GST_EVENT_TYPE (event)) {
  case GST_EVENT_EOS:
    res = start_play_tune (asapdec);
    break;
  case GST_EVENT_NEWSEGMENT:
    res = FALSE;
    break;
  default:
    res = FALSE;
    break;
  }
  gst_event_unref (event);
  gst_object_unref (asapdec);
  return res;
}

static GstFlowReturn
gst_asap_dec_chain (GstPad * pad, GstBuffer * buffer)
{
  GstAsapDec *asapdec = GST_ASAPDEC (gst_pad_get_parent (pad));
  guint64 size = GST_BUFFER_SIZE (buffer);
  if (asapdec->module_len + size > ASAPInfo_MAX_MODULE_LENGTH) {
    GST_ELEMENT_ERROR (asapdec, STREAM, DECODE, (NULL), ("Input file too long"));
    gst_object_unref (asapdec);
    return GST_FLOW_ERROR;
  }
  memcpy (asapdec->module + asapdec->module_len, GST_BUFFER_DATA (buffer), size);
  asapdec->module_len += size;
  gst_buffer_unref (buffer);
  gst_object_unref (asapdec);
  return GST_FLOW_OK;
}

static void
gst_asap_dec_base_init (gpointer gclass)
{
  GstElementClass *element_class = GST_ELEMENT_CLASS (gclass);

  gst_element_class_set_details_simple (element_class,
    "ASAP decoder",
    "Codec/Decoder/Audio",
    "Decodes 8-bit Atari .sap chiptunes",
    "Piotr Fusik <fox@scene.pl>");

  gst_element_class_add_pad_template (element_class,
      gst_static_pad_template_get (&src_factory));
  gst_element_class_add_pad_template (element_class,
      gst_static_pad_template_get (&sink_factory));
}

static void
gst_asap_dec_init (GstAsapDec * asapdec, GstAsapDecClass * gclass)
{
  asapdec->sinkpad = gst_pad_new_from_static_template (&sink_factory, "sink");
  gst_pad_set_event_function (asapdec->sinkpad, GST_DEBUG_FUNCPTR(gst_asap_dec_event));
  gst_pad_set_chain_function (asapdec->sinkpad, GST_DEBUG_FUNCPTR(gst_asap_dec_chain));
  gst_element_add_pad (GST_ELEMENT (asapdec), asapdec->sinkpad);

  asapdec->srcpad = gst_pad_new_from_static_template (&src_factory, "src");
  gst_pad_use_fixed_caps (asapdec->srcpad);
  gst_element_add_pad (GST_ELEMENT (asapdec), asapdec->srcpad);

  asapdec->asap = ASAP_New ();
  asapdec->tune_number = 0;
  asapdec->module_len = 0;
}

static void
gst_asap_dec_finalize (GObject * object)
{
  GstAsapDec *asapdec = GST_ASAPDEC (object);

  ASAP_Delete(asapdec->asap);

  G_OBJECT_CLASS (parent_class)->finalize (object);
}

static void
gst_asap_dec_set_property (GObject * object, guint prop_id,
    const GValue * value, GParamSpec * pspec)
{
  GstAsapDec *asapdec = GST_ASAPDEC (object);
  switch (prop_id) {
  case PROP_TUNE:
    asapdec->tune_number = g_value_get_int (value);
    break;
  default:
    G_OBJECT_WARN_INVALID_PROPERTY_ID (object, prop_id, pspec);
    return;
  }
}

static void
gst_asap_dec_get_property (GObject * object, guint prop_id,
    GValue * value, GParamSpec * pspec)
{
  GstAsapDec *asapdec = GST_ASAPDEC (object);
  switch (prop_id) {
  case PROP_TUNE:
    g_value_set_int (value, asapdec->tune_number);
    break;
  default:
    G_OBJECT_WARN_INVALID_PROPERTY_ID (object, prop_id, pspec);
    return;
  }
}

static void
gst_asap_dec_class_init (GstAsapDecClass * klass)
{
  GObjectClass *gobject_class = (GObjectClass *) klass;

  gobject_class->finalize = gst_asap_dec_finalize;
  gobject_class->set_property = gst_asap_dec_set_property;
  gobject_class->get_property = gst_asap_dec_get_property;

  g_object_class_install_property (G_OBJECT_CLASS (klass), PROP_TUNE,
      g_param_spec_int ("tune", "tune", "tune",
          0, ASAPInfo_MAX_SONGS, 0,
          (GParamFlags) (G_PARAM_READWRITE | G_PARAM_STATIC_STRINGS)));
}

static gboolean
asapdec_init (GstPlugin * asapdec)
{
  /* debug category for filtering log messages */
  GST_DEBUG_CATEGORY_INIT (gst_asap_dec_debug, "asapdec",
      0, "ASAP decoder");

  return gst_element_register (asapdec, "asapdec", GST_RANK_NONE,
      GST_TYPE_ASAPDEC);
}

/* Workaround for sloppy gstconfig.h: it only checks _MSC_VER,
   so it doesn't emit __declspec(dllexport) for MinGW.
   As a result, no symbol has __declspec(dllexport)
   and thus all are exported from the DLL. */
#ifdef _WIN32
#undef GST_PLUGIN_EXPORT
#define GST_PLUGIN_EXPORT __declspec(dllexport)
#endif

/* GST_PLUGIN_DEFINE needs PACKAGE to be defined. */
#define PACKAGE "asap"

/* gstreamer looks for this structure to register asapdec */
GST_PLUGIN_DEFINE (
    GST_VERSION_MAJOR,
    GST_VERSION_MINOR,
    "asapdec",
    "Decodes Atari 8-bit .sap chiptunes",
    asapdec_init,
    ASAPInfo_VERSION,
    "GPL",
    "ASAP",
    "http://asap.sourceforge.net/"
)
