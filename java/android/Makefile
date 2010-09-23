ANDROID_SDK = C:/bin/android-sdk-windows
ANDROID_PLATFORM = $(ANDROID_SDK)/platforms/android-3

JAVAC = javac -d .
AAPT = $(ANDROID_PLATFORM)/tools/aapt
DX = java -jar "$(ANDROID_PLATFORM)/tools/lib/dx.jar" --no-strict
APKBUILDER = java -classpath "$(ANDROID_SDK)/tools/lib/sdklib.jar" com.android.sdklib.build.ApkBuilderMain
JARSIGNER = jarsigner
ADB = $(ANDROID_SDK)/tools/adb
ANDROID = $(ANDROID_SDK)/tools/android.bat
EMULATOR = $(ANDROID_SDK)/tools/emulator

install: AndroidASAP-debug.apk
	$(ADB) -e install -r AndroidASAP-debug.apk

PLAYERS_SRC_DIR = ../../players
PLAYERS_OBX_DIR = players/net/sf/asap

include ../../players/Makefile

log:
	$(ADB) -e logcat

emu:
	$(EMULATOR) -avd myavd &

put-sap:
	$(ADB) push ../../../Drunk_Chessboard.sap /sdcard/

create-avd:
	$(ANDROID) create avd -n myavd -t android-3 -c 16M

AndroidASAP.apk: AndroidASAP-unsigned.apk
	$(JARSIGNER) -signedjar $@ AndroidASAP-unsigned.apk mykey

AndroidASAP-unsigned.apk: AndroidASAP-resources.apk classes.dex
	$(APKBUILDER) $@ -u -z AndroidASAP-resources.apk -f classes.dex

AndroidASAP-debug.apk: AndroidASAP-resources.apk classes.dex
	$(APKBUILDER) $@ -z AndroidASAP-resources.apk -f classes.dex

classes.dex: net/sf/asap/AndroidASAP.class net/sf/asap/R.class
	$(DX) --dex --output=$@ net ../net

net/sf/asap/AndroidASAP.class: AndroidASAP.java net/sf/asap/R.class ../net/sf/asap/ASAP.class
	$(JAVAC) -classpath ".;.." -bootclasspath "$(ANDROID_PLATFORM)/android.jar" AndroidASAP.java

net/sf/asap/R.class: net/sf/asap/R.java
	$(JAVAC) net/sf/asap/R.java

AndroidASAP-resources.apk net/sf/asap/R.java: AndroidManifest.xml res/layout/filelist.xml res/layout/listitem.xml $(PLAYERS_OBX)
	$(AAPT) p -f -m -M AndroidManifest.xml -I "$(ANDROID_PLATFORM)/android.jar" -S res -F AndroidASAP-resources.apk -J . players

../net/sf/asap/ASAP.class:
	$(MAKE) -C .. net/sf/asap/ASAP.class

../net/sf/asap/ASAP_ModuleInfo.class:
	$(MAKE) -C .. net/sf/asap/ASAP_ModuleInfo.class

clean:
	rm -f AndroidASAP.apk AndroidASAP-unsigned.apk AndroidASAP-debug.apk AndroidASAP-resources.apk classes.dex
	rm -rf net

