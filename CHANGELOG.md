# Changelog
All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.0-pre.5] - 2024-01-30
### Fixed
 - Fixed NullReferenceException when the AddressableAssetSettings asset contains a reference to a missing asset group.
 
## [1.0.0-pre.4] - 2023-04-13
### Added
 - Added "Labels" view. It shows bundles grouped by their label. The buildlayout.txt file doesn't include label information, therefore labels are fetched from the project instead. See [PR#4](https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer/pull/4) for details. Thank you vandelpal for the contribution.

## [1.0.0-pre.3] - 2022-03-11
### Added
 - Double-clicking an asset in BuildLayout Explorer is now trying to select the asset in the Unity project window.
 - Added "Copy" context menu item to every list, can be used to copy the text of a cell to clipboard.
 - Added "Copy Row" context menu item to every list, can be used to copy the text of a row to clipboard.

### Changed
 - Changed how lookup data is stored to improve load performance. See [PR#2](https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer/pull/2) for details. Thank you zsoi for the contribution.

## [1.0.0-pre.2] - 2021-08-30
### Added
 - Added functionality to select an asset in the project. It's the little magnifying-glass icon next to an asset.
 - Added link to Unity forum post to provide feedback regarding BuildLayout Explorer.

## [1.0.0-pre.1] - 2021-08-27
### Added
 - Public release
