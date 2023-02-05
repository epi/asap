ASMA_DIR = ../aasma/asma
ANDROID_SDK = C:/Users/fox/AppData/Local/Android/Sdk
ANDROID_JAR = $(ANDROID_SDK)/platforms/android-33/android.jar
ANDROID_BUILD_TOOLS = $(ANDROID_SDK)/build-tools/33.0.0

JAVA = $(DO)java
AAPT = $(ANDROID_BUILD_TOOLS)/aapt
D8 = $(DO)java -cp "$(ANDROID_BUILD_TOOLS)/lib/d8.jar" com.android.tools.r8.D8
APKSIGNER = $(DO)$(ANDROID_BUILD_TOOLS)/apksigner.bat
ZIPALIGN = $(DO)$(ANDROID_BUILD_TOOLS)/zipalign
ADB = $(ANDROID_SDK)/platform-tools/adb
ANDROID = $(ANDROID_SDK)/tools/android.bat

# NDK is only needed for the command-line asapconv
# It ended up in this Makefile even though it's not Java
ANDROID_NDK = C:/bin/android-ndk-r8c
ANDROID_NDK_PLATFORM = $(ANDROID_NDK)/platforms/android-3/arch-arm
ANDROID_CC = $(DO)$(ANDROID_NDK)/toolchains/arm-linux-androideabi-4.6/prebuilt/windows/bin/arm-linux-androideabi-gcc --sysroot=$(ANDROID_NDK_PLATFORM) -s -O2 -Wall -o $@ $(INCLUDEOPTS) $(filter %.c,$^)

# no user-configurable paths below this line

ifndef DO
$(error Use "Makefile" instead of "android.mk")
endif

ANDROID_RELEASE = release/asap-$(VERSION)-android.apk
ANDROID_JAVA_SRC = $(addprefix $(srcdir)java/android/, ArchiveSuggestionsProvider.java FileInfo.java Player.java PlayerService.java Util.java)

android-release: $(ANDROID_RELEASE)
.PHONY: android-release

android-install: $(ANDROID_RELEASE)
	$(ADB) install -r $<
.PHONY: android-install

android-log:
	$(ADB) logcat -d
.PHONY: android-log

android-log-clear:
	$(ADB) logcat -c
.PHONY: android-log-clear

$(ANDROID_RELEASE): java/android/AndroidASAP-unsigned.apk
	$(APKSIGNER) sign --ks C:/Users/fox/.keystore --ks-key-alias pfusik --ks-pass pass:walsie --out $@ $<

java/android/AndroidASAP-unsigned.apk: java/android/AndroidASAP-unaligned.apk
	$(ZIPALIGN) -f 4 $< $@
CLEAN += java/android/AndroidASAP-unsigned.apk

java/android/AndroidASAP-unaligned.apk: java/android/AndroidASAP-resources.apk java/android/classes.dex
	$(DO)cp $< $@ && $(SEVENZIP) -tzip $@ ./java/android/classes.dex
CLEAN += java/android/AndroidASAP-unaligned.apk

java/android/classes.dex: java/android/classes/net/sf/asap/Player.class
	$(D8) --release --output $(@D) --lib $(ANDROID_JAR) `ls java/android/classes/net/sf/asap/*.class`
CLEAN += java/android/classes.dex

java/android/classes/net/sf/asap/Player.class: $(ANDROID_JAVA_SRC) java/android/AndroidASAP-resources.apk java/src/net/sf/asap/ASAP.java
	$(JAVAC) -d java/android/classes --release 11 -cp $(ANDROID_JAR) -Xlint:deprecation $(ANDROID_JAVA_SRC) java/android/gen/net/sf/asap/R.java java/src/net/sf/asap/*.java
CLEANDIR += java/android/classes

# Also generates java/android/gen/net/sf/asap/R.java
java/android/AndroidASAP-resources.apk: $(addprefix $(srcdir)java/android/,AndroidManifest.xml \
	res/drawable/ic_menu_browse.png res/drawable-land/background.jpg res/drawable-port/background.jpg res/drawable/banner.png res/drawable/icon.xml res/drawable/list_selector.xml res/drawable/list_selector_focused.xml \
	res/layout/fileinfo_list_item.xml res/layout/shuffle_all_list_item.xml res/layout/player.xml res/layout-land/fileinfo_list_item.xml res/layout-land/player.xml res/menu/player.xml res/values/strings.xml res/values/themes.xml res/xml/searchable.xml) \
	java/android/res/drawable-land/background.jpg java/android/res/drawable-port/background.jpg java/android/res/drawable-land/stereo.jpg java/android/res/drawable-port/stereo.jpg \
	$(ASMA_DIR)/index.txt $(JAVA_OBX)
	$(DO)mkdir -p java/android/gen && $(AAPT) p -f -m -M $< -I $(ANDROID_JAR) -S $(srcdir)java/android/res -A $(ASMA_DIR) --ignore-assets Docs:*.ttt -F $@ -J java/android/gen java/obx
CLEAN += java/android/AndroidASAP-resources.apk java/android/gen/net/sf/asap/R.java

$(ASMA_DIR)/index.txt: java/android/Indexer.class
	$(JAVA) -classpath "java/android;java/classes" Indexer $(ASMA_DIR) | dos2unix >$@
CLEAN += $(ASMA_DIR)/index.txt

java/android/Indexer.class: $(srcdir)java/android/Indexer.java java/classes/net/sf/asap/ASAP.class
	$(JAVAC) -d $(@D) -classpath java/classes $<
CLEAN += java/android/Indexer.class

java/android/res/drawable-land/background.jpg: java/android/img/POKEY_chip_on_an_Atari_130XE_motherboard.jpg
	$(DO)magick $< -crop 2560x1280+0+420 -resize 2160x1080 -brightness-contrast -70x-75 $@
CLEAN += java/android/res/drawable-land/background.jpg

java/android/res/drawable-port/background.jpg: java/android/img/POKEY_chip_on_an_Atari_130XE_motherboard.jpg
	$(DO)magick $< -crop 928x1856+112+0 -resize 1080x2160 -brightness-contrast -70x-75 $@
CLEAN += java/android/res/drawable-port/background.jpg

java/android/res/drawable-land/stereo.jpg: java/android/img/stereo.jpg
	$(DO)magick $< -crop 3840x1920+192+380 -resize 2160x1080 -brightness-contrast -70x-75 $@
CLEAN += java/android/res/drawable-land/stereo.jpg

java/android/res/drawable-port/stereo.jpg: java/android/img/stereo.jpg
	$(DO)magick $< -crop 1512x3024+900+0 -resize 1080x2160 -brightness-contrast -70x-75 $@
CLEAN += java/android/res/drawable-port/stereo.jpg

android-push-asapconv: java/android/asapconv
	$(ADB) -d push java/android/asapconv /data/local/tmp/
.PHONY: android-push-asapconv

java/android/asapconv: $(call src,asapconv.c asap.[ch])
	$(ANDROID_CC)
CLEAN += java/android/asapconv
