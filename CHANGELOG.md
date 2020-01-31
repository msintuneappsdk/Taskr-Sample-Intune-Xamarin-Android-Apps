# Version 2.3

* Revert update to the basic Android sample app that introduced the Remapper for MAM replacements.
* Convert change log to markdown file.

# Version 2.2

* Update MAM bindings and MAM Remapper NuGet packages to the latest release.
* Resolve Forms remapping issue in Forms versions 4.3+ with `FormsAppCompatActivity`.

# Version 2.1

* Update MAM bindings and MAM Remapper NuGet packages to the latest release.
* Resolve Forms remapping issue in latest versions of VS 2019.
* Update to latest ADAL release.

# Version 2.0

* Update basic Android sample app to use the Remapper for MAM class, method, and system services replacements.

# Version 1.3

* Update sample apps to target Android P.
* Update MAM bindings and MAM Remapper NuGet packages to the latest release.
* Update support libraries and Xamarin.Forms version.
* Update code for MAM print policy enforcement.
* Remove Resource.Designer.cs files from source control and add to git ignore.

# Version 1.2

* A known issue with the Xamarin bound MAM SDK would cause the application to crash when deployed in Debug mode. This change provides a workaround for that issue by adding an application attribute that enables the application to be debugged without it crashing.

# Version 1.1

* Ensure application authentication values are properly configured before attempting authentication to aide in development and prevent application crashes.

# Version 1.0

* Initial release of Taskr.
