ANDROID_SDK = C:/bin/android-sdk-windows
ANDROID_PLATFORM = $(ANDROID_SDK)/platforms/android-8

AAPT = $(ANDROID_PLATFORM)/tools/aapt
DX = $(DO)java -jar "$(ANDROID_PLATFORM)/tools/lib/dx.jar" --no-strict
APKBUILDER = $(DO)java -classpath "$(ANDROID_SDK)/tools/lib/sdklib.jar" com.android.sdklib.build.ApkBuilderMain $@
JARSIGNER = $(DO)jarsigner
ZIPALIGN = $(DO)$(ANDROID_SDK)/tools/zipalign
ADB = $(ANDROID_SDK)/tools/adb
ANDROID = $(ANDROID_SDK)/tools/android.bat
EMULATOR = $(ANDROID_SDK)/tools/emulator

# no user-configurable paths below this line

ifndef DO
$(error Use "Makefile" instead of "android.mk")
endif

ANDROID_RELEASE = release/asap-$(VERSION)-android.apk

android-debug: java/android/AndroidASAP-debug.apk
.PHONY: android-debug

android-release: $(ANDROID_RELEASE)
.PHONY: android-release

android-install-emu: java/android/AndroidASAP-debug.apk
	$(ADB) -e install -r java/android/AndroidASAP-debug.apk
.PHONY: android-install-emu

android-install-dev: $(ANDROID_RELEASE)
	$(ADB) -d install -r $(ANDROID_RELEASE)
.PHONY: android-install-dev

android-log-emu:
	$(ADB) -e logcat
.PHONY: android-log-emu

android-log-dev:
	$(ADB) -d logcat
.PHONY: android-log-dev

android-shell:
	$(ADB) -d shell
.PHONY: android-shell

android-emu:
	$(EMULATOR) -avd myavd &
.PHONY: android-emu

android-push-release: $(ANDROID_RELEASE)
	$(ADB) -d push $(ANDROID_RELEASE) /sdcard/sap/
.PHONY: android-push-release

android-push-sap:
	$(ADB) -e push ../Drunk_Chessboard.sap /sdcard/
.PHONY: android-push-sap

android-create-avd:
	$(ANDROID) create avd -n myavd -t android-4 -c 16M
.PHONY: android-create-avd

$(ANDROID_RELEASE): java/android/AndroidASAP-unaligned.apk
	$(ZIPALIGN) -f 4 $< $@

java/android/AndroidASAP-unaligned.apk: java/android/AndroidASAP-unsigned.apk
	$(JARSIGNER) -storepass walsie -signedjar $@ java/android/AndroidASAP-unsigned.apk pfusik-android
CLEAN += java/android/AndroidASAP-unaligned.apk

java/android/AndroidASAP-unsigned.apk: java/android/AndroidASAP-resources.apk java/android/classes.dex
	$(APKBUILDER) -u -z java/android/AndroidASAP-resources.apk -f java/android/classes.dex
CLEAN += java/android/AndroidASAP-unsigned.apk

java/android/AndroidASAP-debug.apk: java/android/AndroidASAP-resources.apk java/android/classes.dex
	$(APKBUILDER) -z java/android/AndroidASAP-resources.apk -f java/android/classes.dex
CLEAN += java/android/AndroidASAP-debug.apk

java/android/classes.dex: java/android/classes/net/sf/asap/Player.class
	$(DX) --dex --output=$@ java/android/classes
CLEAN += java/android/classes.dex

java/android/classes/net/sf/asap/Player.class: $(addprefix $(srcdir)java/android/,FileSelector.java MainMenu.java Player.java) java/android/AndroidASAP-resources.apk java/src/net/sf/asap/ASAP.java
	$(JAVAC) -d java/android/classes -bootclasspath "$(ANDROID_PLATFORM)/android.jar" $(addprefix $(srcdir)java/android/,FileSelector.java MainMenu.java Player.java) java/android/src/net/sf/asap/R.java java/src/net/sf/asap/*.java
CLEANDIR += java/android/classes

# Also generates java/android/src/net/sf/asap/R.java
java/android/AndroidASAP-resources.apk: $(addprefix $(srcdir)java/android/,AndroidManifest.xml res/drawable/icon.png res/layout/error.xml res/layout/list_item.xml res/layout/playing.xml res/values/strings.xml res/values/themes.xml) $(JAVA_OBX)
	$(DO)mkdir -p java/android/src && $(AAPT) p -f -m -M $< -I $(ANDROID_PLATFORM)/android.jar -S $(srcdir)java/android/res -F $@ -J java/android/src java/obx
CLEAN += java/android/AndroidASAP-resources.apk java/android/src/net/sf/asap/R.java
