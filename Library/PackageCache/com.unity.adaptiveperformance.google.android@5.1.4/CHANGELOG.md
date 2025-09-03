# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [5.1.4] - 2025-04-15

### Changed
* Requesting the Thermal headroom every 10 sec.

### Fixed
* Fixed problem with Temperature trend not being reported.

## [5.1.3] - 2025-03-13

### Fixed
* Fixed problem with "Exception: Field SDK_INT or type signature not found" when using Editor PlayMode.

## [5.1.2] - 2025-02-14

### Changed
* Min supported Unity version increased to 2022.3.
* Updated Performance Hint API logic:
  * dialog box requesting to turn on Frame Stats when provider is installed or updated.
  * creating two Performance sessions, reporting full frame time to the first session, max of the CPU times for the Unity main thread and for the Graphics thread to the second session.
  * reporting the target CPU times for both sessions only when the target FPS is changed.
  * supporting new Android 15 (API 35) specific methods to report the GPU time to the first session.
* Updated Thermal API logic:
  * requesting the Thermal headroom every 12 sec (1 sec on Android 15).
* Support Android 16 KB page size devices by aligning `libAdaptivePerformanceHint.so` and `libAdaptivePerformanceThermalHeadroom.so` to 16 KB

### Fixed
* Fixed problem with "DllNotFoundException: Unable to load DLL 'AdaptivePerformanceThermalHeadroom'" when running on Android 11 devices.
* Fixed problem with extra memory allocation when reporting CPU/GPU times using Performance Hint Manager.
* Requesting GameMode only when app starts or resumes.

## [1.3.1] - 2024-03-27

### Changed
* Native libraries are automatically excluded from the build if provider is disabled in Adaptive Performance settings.

## [1.3.0] - 2024-03-12

### Added
* Enabled a new property on `IAdaptivePerformanceSettings` to automate frame rate control based on the changes in device performance mode.

### Removed
* `SetGameState` call on Adaptive Performance initialization to allow automated `SetGameState` calls from the Unity Player.

## [1.2.1] - 2023-08-28

### Fixed
* Some deviced do not report correct thermal headroom numbers. Disabling Adaptive Performance on those devices.

## [1.2.0] - 2023-08-03

### Changed
* Unity Support Version dropped to 2021

## [1.1.2] - 2023-06-06

### Fixed
* Fixed build error "Frametiming does not contain a definition for 'cpumainthreadframetime' on Unity versions < 2022.1.

## [1.1.1] - 2023-06-14

### Fixed
* Fixed wrong threshold for throttling where Android Moderate throttling reports now as Adaptive Performance throttling instead of throttling imminent.
* Fix time reporting to Android performance hint manager to report main thread and render thread timing correctly.

## [1.1.0] - 2023-04-17

### Changed
* Rework how Android APIs of Android 11, 12, 13 are integrated and used so we can target Android 11 and not only Android 12+ to provide minimal features sets to the platform (thermal events).


### Fixed
* Fixed Android APIs to ensure we run on any Android device not only Google Pixel devices.


## [1.0.0] - 2023-04-10

### Added
* Support for Android SDK 33
* Integrated Android APIs
  * [GameManager](https://developer.android.com/reference/android/app/GameManager) APIs
    * [getGameMode](https://developer.android.com/reference/android/app/GameManager#getGameMode())
    * [setGameState](https://developer.android.com/reference/android/app/GameManager#setGameState(android.app.GameState))
  * [PowerManager](https://developer.android.com/reference/android/os/PowerManager) APIs
    * [getThermalHeadroom](https://developer.android.com/reference/android/os/PowerManager#getThermalHeadroom(int))
    * [getCurrentThermalStatus](https://developer.android.com/reference/android/os/PowerManager#getCurrentThermalStatus())
    * [ThermalStatusChangedListener](https://developer.android.com/reference/android/os/PowerManager.OnThermalStatusChangedListener)
  * [PerformanceHintManager](https://developer.android.com/reference/android/os/PerformanceHintManager) APIs
    * [createHintSession](https://developer.android.com/reference/android/os/PerformanceHintManager#createHintSession(int[],%20long))
    * [getPreferredUpdateRateNanos](https://developer.android.com/reference/android/os/PerformanceHintManager#getPreferredUpdateRateNanos())
  * [PerformanceHintManager.Session](https://developer.android.com/reference/android/os/PerformanceHintManager.Session) APIs
    * [reportActualWorkDuration](https://developer.android.com/reference/android/os/PerformanceHintManager.Session#reportActualWorkDuration(long))

### Removed
* Removed Project Settings options that are not available in this Provider.

### Changed
* Adjusted labeling of new Android provider to be listed as Android provider.

### Fixed
- Adjusted the loader and subsystem initialization process to allow for falling back to another subsystem if init is not successful.
- Fixed exception in simulator attempting to load game mode on Editor versions prior to 2023.1
