# Mobile Green Steaming

## About The Project

With the rise of mobile streaming, understanding and optimizing the energy consumption of mobile devices during content playback is crucial for both user experience and sustainability. The project aims to investigate the availability of electricity consumption data on mobile devices and experiment with various options to reduce energy usage while maintaining a high-quality streaming experience.


## Build Instructions

Pre-requisites:
* Android SDK
* To run command line tools, you'll need to configure Java: see [guide](https://github.com/mozilla-mobile/shared-docs/blob/master/android/configure_java.md).

1. Clone or Download the repository:

  ```shell
  git clone https://github.com/julioc-p/awt-pj-ws23-24-mobile-streaming-1
  ```

2. **Import** the project into Android Studio **or** build on the command line:

  ```shell
  ./gradlew clean app:assembleDebug
  ```

If this errors out, make sure that you have an `ANDROID_SDK_ROOT` environment
variable pointing to the right path.
