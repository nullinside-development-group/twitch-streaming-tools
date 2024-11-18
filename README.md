# nullinside-template-desktop-gui

This is a template for creating desktop applications for C# using Avalonia.

## Setup

1. When creating a new git repo you can select the `nullinside-template-desktop-gui` project as your template.
2. Perform a rename through the application files for the following:
	1. `[ApplicationNameUpperCamelCase]`: The desired name of the application in UpperCamelCase format.
	2. `[GitOwnerAndRepoName]`: The github owner and repo combination name. (Ex: `nullinside-development-group/nullinside-site-monitor`)
	3. Update the namespaces in the application using an IDE.
	4. Update the name of the documentation file in the project settings under Debug and Release
 	5. Update the git repo location in the new version dialog's view model: https://github.com/nullinside-development-group/nullinside-template-desktop-gui/blob/main/src/%5BApplicationNameUpperCamelCase%5D/ViewModels/NewVersionWindowViewModel.cs#L44
