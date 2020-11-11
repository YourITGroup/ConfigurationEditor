# Configuration Editor

![Configuration Editor Logo](https://raw.githubusercontent.com/YourITGroup/ConfigurationEditor/master/assets/ConfigurationManager_logo.png)

Nuget Package: 
[![NuGet release](https://img.shields.io/nuget/v/ConfigurationEditor.svg)](https://www.nuget.org/packages/ConfigurationEditor/)
[![NuGet release](https://img.shields.io/nuget/dt/ConfigurationEditor.svg)](https://www.nuget.org/packages/ConfigurationEditor/)

Umbraco Package:
[![Our Umbraco project page](https://img.shields.io/badge/our-umbraco-orange.svg)](https://our.umbraco.org/projects/backoffice-extensions/configuration-editor) 

Adds a Configuration Editor to the Settings section for Umbraco 8 with capability to edit all files in the /config directory plus the root Web.Config file.  The root Web.Config file will be backed up with a date-stamped extension on save.

Compiled against Umbraco 8.6.4

## Sample Web project:

* Uses SqlCe database - username is "**admin@admin.com**"; password is "**Password123**"
* Umbraco 8.6.4

## RoadMap / Releases:

### Release 0.1.5

* [x] Expanded configuration filters to include all configuration files under App_Plugins (json, xml, config)

### Release 0.1.4

* [x] New Logo 💯

### Release 0.1.3

* [x] Moved alert banner for Web.config so it doesn't overlap the actual editing area obscuring it.

### Release 0.1.2

* [x] Remove empty folders
* [x] Import proper Ace Editor xml view instead of using Razor.

## Logo
The package logo uses the Configuration (by Joe Pictos) icon from the Noun Project, licensed under CC BY 3.0 US.
