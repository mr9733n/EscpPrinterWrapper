# Changelog

All notable changes to this project will be documented in this file.

## [1.0.0] - 2024-06-23

### Added
- Initial release with text and barcode wrapping functionality.
- Support for generating full print commands with options like cut paper and landscape orientation.

### Fixed
- N/A

### Changed
- N/A

## [1.0.1] - 2024-06-23

### Added
- Additional font support.
- Carriage return support.

### Fixed
- N/A

### Changed
- Code refactoring for improved readability and maintainability.

## [1.0.2] - 2024-06-23

### Added
- Support for horizontal tabulation.
- Ability to input and wrap two text strings.

### Fixed
- Correct handling of text input with single quotes.

### Changed
- Further code refactoring for optimization.
- More tests

## [1.0.3] - 2024-06-23

### Fixed
- Code refactoring for improved readability and maintainability.

## [1.0.4] - 2024-06-23

### Added
- Changed constants to byte arrays for ESC/P commands.

### Fixed
- Refactored code for improved readability and maintainability.
- Fixed tests to correctly handle expected and actual values for byte arrays in ESC/P commands.
- Adjusted logging to accurately represent the byte array commands being processed.
- Updated WrapText and WrapBarcode methods to use byte arrays and fixed issues with command generation.
