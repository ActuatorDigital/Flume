# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- FlumeServiceContainer can now auto create ScriptableObject TImplementations, as it does for MonoBehaviour.
- Add CHANGELOG.md.
- Add README.md.

### Changed

- Injector now caches type and method results between calls, improves Inject speed and greatly reduces memory thrashing.
- ServiceRegister now calls Dispose on all contained IDisposable services.

### Fixed

- FlumeServiceContainer statics are now automatically cleared between test runs via the added TestListerner in AID.Flume.Editor.
- Subscribing to OnContainerReady after it has already signaled, now invokes the subscriber immediately.

## [0.0.4] - 2021-01-19
