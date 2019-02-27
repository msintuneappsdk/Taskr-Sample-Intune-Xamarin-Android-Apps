# Taskr - A [Microsoft Intune](https://www.microsoft.com/en-us/cloud-platform/microsoft-intune) Xamarin SDK Example
This repository is a demonstration of the [Microsoft Intune App SDK](https://docs.microsoft.com/en-us/intune/app-sdk) with Xamarin for Android. A developer guide to the SDK is available [here](https://docs.microsoft.com/en-us/intune/app-sdk-xamarin). Both sample apps, basic Xamarin.Android and Xamarin.Forms, implement commonly used features so developers making their own apps have an example to follow. IT administrators who want to create apps with similar functionality can even use these apps as a template.

Taskr allows users to keep a list of to-do items, or tasks. Users can view their open tasks and mark tasks as complete, print them, or save them to their phone. Tasks are kept in a database implemented using [SQLite](https://docs.microsoft.com/en-us/xamarin/android/data-cloud/data-access/using-sqlite-orm). Users' actions are managed by policy, so not all actions may be available.

## Important Notes Before Starting
### Configuring an Intune Subscription
- A tenant is necessary for the configuration of an Intune subscription. A free trial is sufficient for this demo and can be registered for at [Microsoft's demo site](https://demos.microsoft.com).
- Once a tenant is acquired the Intune subscription will need to be properly configured to target the user and the application. Follow the set up steps found [here](https://docs.microsoft.com/en-us/intune/setup-steps).
### Configuring App for ADAL Authentication
- Perform the app registration and configuration steps found [here](https://github.com/Azure-Samples/active-directory-android#register--configure-your-app). 
  - The purpose of registering with ADAL is to acquire a client ID and redirect URI for your application. Once you have registered your app, replace the client ID and the redirect URI.
  - For the Xamarin.Android app, replace `_clientID` in `TaskrAndroid\Authentication\AuthManager.cs` and `clientId` in `TaskrAndroid\Properties\AndroidManifest.xml` with the acquired ID.
  - For the Xamarin.Forms app, replace `ClientID ` in `TaskrForms\TaskrForms\App.xaml.cs` and `clientID` in `TaskrForms.Android\Properties\AndroidManifest.xml` with the acquired ID.
### Grant App Permission to MAM Service
- You will need to [grant your app permissions](https://docs.microsoft.com/en-us/intune/app-sdk-get-started#give-your-app-access-to-the-intune-app-protection-service-optional) to the Intune Mobile Application Management (MAM) service.

## Highlighted SDK Features
This project demonstrates proper integration with the MAM SDK and the [MAM-WE service](https://docs.microsoft.com/en-us/intune/app-sdk-android#app-protection-policy-without-device-enrollment). However, it does not show how to properly handle [multi-identity](https://docs.microsoft.com/en-us/intune/app-sdk-android#multi-identity-optional) protection. If your application needs to be multi-identity aware please refer to the [implementation documentation](https://docs.microsoft.com/en-us/intune/app-sdk-android#enabling-multi-identity).

__! NOTE__ For policy to be applied to the application, the user will need to sign in and authenticate with ADAL. 

### Actively Managed
The following policies require explicit app involvement in order to be properly enforced. 

- Prevent Android backups – The app enables managed backups in `AndroidManifest.xml`. More information is available [here](https://docs.microsoft.com/en-us/intune/app-sdk-android#protecting-backup-data).
- Prevent "Save As": 
  - To User's Device - To determine if saving to the device is allowed, the app manually checks the user's policy. If allowed, the save button will save a CSV containing all open tasks to the user's device. Otherwise, a notification will be displayed to the user.
    - Xamarin.Android: `Fragments\TasksFragment.cs`
    - Xamarin.Forms: `TaskrForms.Android\SaveUtility.cs`
- App configuration policies – The app displays the current configuration as an example on the About page.
    - Xamarin.Android: `Fragments\AboutFragment.cs`
    - Xamarin.Forms: `TaskrForms.Android\ConfigUtility.cs`

### Automatically Managed
The following policies are automatically managed by the SDK without explicit app involvement and require no additional development.

- Require PIN for access – The MAM SDK will prompt the user for a PIN before any UI code is executed, if required by policy.
  - Allow fingerprint instead of PIN - See above.
  - Require corporate credentials for access – See above.
- Allow app to transfer data to other apps – This policy is demonstrated when the user clicks on the save button, which attempts to export a CSV containing tasks to Excel.
- Disable printing – This policy is demonstrated when the user clicks on the print button, which attempts to open the CSV in Android's default printing view.
- Allow app to receive data from other apps – This policy is demonstrated when the app receives intents containing the text of a description to create a task.
- Restrict web content to display in the [Managed Browser](https://docs.microsoft.com/en-us/intune/app-configuration-managed-browser) – This policy is demonstrated when a user clicks on a link from the About screen.
- Encrypt app data - This policy is demonstrated when the app attempts to save a CSV file. If enabled, the file will be encrypted on disk.

## Relevant Files
### Xamarin.Android Application - `TaskrAndroid\`
- `AndroidManifest.xml` requests the necessary permissions and sets up the MAM SDK's backup manager.
- `MainActivity.cs` contains the high-level flow for authentication & account registration.
- `TaskrApplication.cs` is the required Application class that inherits from `MAMApplication` and registers notification receivers.
- `Authentication\AuthManager.cs` contains the bulk of the ADAL authentication logic.
- `Authentication\MAMWEAuthCallback.cs` is the required callback for MAM account registration.
- `Fragments\TasksFragment.cs` explicitly checks MAM policies to see if saving files to a user's device is allowed.
- `Fragments\AboutFragment.cs` attempts to retrieve and display the user's Application Configuration JSON object.
- `Receivers\EnrollmentNotificationReceiver.cs` and `Receivers\ToastNotificationReceiver.cs` receive and handle notifications sent by MAM.

### Xamarin.Forms Android Application - `TaskrForms.Android\`
- `AndroidManifest.xml` requests the necessary permissions and sets up the MAM SDK's backup manager.
- `MainActivity.cs` contains the high-level flow for authentication & account registration.
- `TaskrApp.cs` is the required Application class that inherits from `MAMApplication` and registers notification receivers.
- `Authentication\Authenticator.cs` contains the bulk of the ADAL authentication logic.
- `Authentication\MAMWEAuthCallback.cs` is the required callback for MAM account registration.
- `SaveUtility.cs` explicitly checks MAM policies to see if saving files to a user's device is allowed.
- `ConfigUtility.cs` attempts to retrieve and display the user's Application Configuration JSON object.
- `Receivers\EnrollmentNotificationReceiver.cs` and `Receivers\ToastNotificationReceiver.cs` receive and handle notifications sent by MAM.

## Troubleshooting
- The apps cannot be debugged with the Company Portal installed on API 23+ due to an issue in the [Microsoft Intune App SDK Xamarin Bindings](https://github.com/msintuneappsdk/intune-app-sdk-xamarin).
