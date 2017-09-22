﻿using System;
using System.IO;
using System.Net;
using GalleryServer.Business.Entity;
using GalleryServer.Business.Interfaces;
using GalleryServer.Data;
using GalleryServer.Events;
using Newtonsoft.Json;

namespace GalleryServer.Business
{
  /// <summary>
  /// Represents a license for the Gallery Server software.
  /// </summary>
  public class License : ILicense
  {
    private static readonly object _sharedLock = new object();

    /// <summary>
    /// Gets or sets the email used to purchase the license key. Ignored for free licenses.
    /// </summary>
    /// <value>The license email.</value>
    public string LicenseEmail { get; set; }

    /// <summary>
    /// Gets or sets the license key.
    /// </summary>
    /// <value>The license key.</value>
    public string LicenseKey { get; set; }

    /// <summary>
    /// Gets or sets the instance ID. This is a string generated by the license server that uniquely identifies an activated
    /// installation. It will be empty for the trial and free versions.
    /// </summary>
    /// <value>The license key.</value>
    public string InstanceId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the license contained in this instance is legitimate and authorized.
    /// </summary>
    /// <value><c>true</c> if the license is valid; otherwise, <c>false</c>.</value>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets a message explaining why the key is invalid or why a deactivation failed. Will be blank when activation
    /// or deactivation is successful.
    /// </summary>
    /// <value>A string explaining why the key is invalid or why a deactivation failed.</value>
    public string KeyInvalidReason { get; set; }

    /// <summary>
    /// Gets or sets the application version the license applies to. Example: 4.0.0, 4.1.2
    /// </summary>
    /// <value>The application version the license applies to.</value>
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the type of the license applied to the current application.
    /// </summary>
    /// <value>The type of the license.</value>
    public LicenseLevel LicenseType { get; set; }

    /// <summary>
    /// Gets the date/time this application was installed. The timestamp of the oldest gallery's creation date is
    /// considered to be the application install date.
    /// </summary>
    /// <value>The date/time this application was installed.</value>
    public DateTime InstallDate { get; set; }

    /// <summary>
    /// Gets the full file path to where the version key file is expected to be. Does not verify the file exists.
    /// Ex: "C:\inetpub\wwwroot\Website\App_Data\version_key.txt"
    /// </summary>
    /// <value>The version key file path.</value>
    public string VersionKeyFilePath
    {
      get
      {
        return System.IO.Path.Combine(AppSetting.Instance.PhysicalApplicationPath, GlobalConstants.AppDataDirectory, GlobalConstants.VersionKeyFileName);
      }
    }


    /// <summary>
    /// Populate the calculated properties based on the properties retrieved from the data store. It is intended that this
    /// method is invoked during application initialization or after a license deactivation.
    /// </summary>
    public void Inflate()
    {
      KeyInvalidReason = string.Empty;

      // 1. If key is blank, then license is either Trial or TrialExpired
      // 2. If key matches free version key, then it's valid (with a quick check to make sure user isn't using SQL Server).
      // 3. For all other values, check AppSetting.VersionKey or version_key.txt. Verify version matches actual running version.
      // 4. If so, and if a license key exists, then the license is valid.

      if (string.IsNullOrWhiteSpace(LicenseKey))
      {
        ValidateBlankLicenseKey();
        return;
      }

      //if (LicenseKey.Equals(Constants.LicenseKeyFree, StringComparison.OrdinalIgnoreCase))
      //{
      //  ValidateFreeLicenseKey();
      //  return;
      //}

      if (ValidateVersionKey())
      {
        if (LicenseType == LicenseLevel.Free)
        {
          ValidateFreeLicenseKey();
          return;
        }

        // If we get here, we have a valid version key, so if we also have a license key (which is only stored when it's valid), we must be good!
        if (!string.IsNullOrWhiteSpace(LicenseKey))
        {
          IsValid = true;
        }
      }
    }

    /// <summary>
    /// Verify whether the <paramref name="licenseEmail" /> and <paramref name="licenseKey" /> is valid. This method stores the 
    /// parameters on the matching instance properties (even if activation fails) and updates <see cref="IsValid" />,
    /// <see cref="KeyInvalidReason" />, and <see cref="LicenseType" />, which may be inspected for follow-up action.
    /// </summary>
    /// <param name="licenseEmail">The license email.</param>
    /// <param name="licenseKey">The license key to validate.</param>
    /// <param name="appUrl">The URL to the root of the gallery application.</param>
    public void Activate(string licenseEmail, string licenseKey, string appUrl)
    {
      LicenseEmail = licenseEmail;
      LicenseKey = licenseKey;

      KeyInvalidReason = string.Empty;

      // 1. If key is blank, then license is either Trial or TrialExpired
      // 2. If key matches free version key, then it's valid (with a quick check to make sure user isn't using SQL Server).
      // 3. For all other values, check AppSetting.VersionKey or version_key.txt. Verify version matches actual running version.
      // 4. If so, then activate against server. Send email, license key, product ID (from VersionKey), and URL

      if (string.IsNullOrWhiteSpace(LicenseKey))
      {
        DeactivateCurrentLicense();

        ValidateBlankLicenseKey();

        return;
      }

      //if (LicenseKey.Equals(Constants.LicenseKeyFree, StringComparison.OrdinalIgnoreCase))
      //{
      //  DeactivateCurrentLicense();

      //  ValidateFreeLicenseKey();

      //  LicenseKey = Constants.LicenseKeyFree; // Fix any casing differences

      //  return;
      //}

      if (ValidateVersionKey())
      {
        DeactivateCurrentLicense();

        ActivateLicenseKey(appUrl);
      }
    }

    /// <summary>
    /// Deactivate the current license key stored in <see cref="AppSetting.LicenseKey" />. It is expected this function is called when 
    /// changing from an activated key to another key.
    /// </summary>
    private void DeactivateCurrentLicense()
    {
      if (!string.IsNullOrWhiteSpace(AppSetting.Instance.LicenseKey))
      {
        // User is changing a license key. Deactivate the current one.
        var oldLicenseEmail = LicenseEmail;
        var oldLicenseKey = LicenseKey;

        LicenseEmail = AppSetting.Instance.LicenseEmail;
        LicenseKey = AppSetting.Instance.LicenseKey;

        Deactivate();

        KeyInvalidReason = string.Empty; // Ignore any errors during deactivation by clearing KeyInvalidReason (event log will have an entry)

        LicenseEmail = oldLicenseEmail;
        LicenseKey = oldLicenseKey;
      }
    }

    /// <summary>
    /// Deactivates a license that was previously activated.
    /// </summary>
    /// <returns><c>true</c> if deactivation successful, <c>false</c> otherwise.</returns>
    public bool Deactivate()
    {
      // Call web service to deactivate key, then clear out LicenseKey, LicenseEmail, and InstanceId
      // API info: https://docs.woothemes.com/document/software-add-on/
      //http://dev.galleryserverpro.com/woocommerce/?wc-api=software-api&request=deactivation&email=Nixsky84@gmail.com&licence_key=GS-9659000f-f127-45a5-93ac-8d70d99b6adf&product_id=Gallery Server Enterprise&platform=http://localhost/dev/gs/default.aspx&instance=1456851984
      var url = $"{GlobalConstants.LicenseServerUrl}?wc-api=software-api&request=deactivation&email={Uri.EscapeDataString(LicenseEmail)}&licence_key={Uri.EscapeDataString(LicenseKey)}&instance={Uri.EscapeDataString(InstanceId)}&product_id={Uri.EscapeDataString(LicenseType.GetDescription())}";
      // For debug purposes - causes error: var url = $"{LicenseServerUrl}?wc-api=software-api&request=deactivation&email={Uri.EscapeDataString(LicenseEmail)}&licence_key={Uri.EscapeDataString(LicenseKey)}&instance={Uri.EscapeDataString(InstanceId)}";

      LicenseServerResult licenseServerResult = null;

      string response = null;
      if (InstanceId != GlobalConstants.LicenseActivationFailedInstanceId)
      {
        using (var client = new WebClient())
        {
          try
          {
            response = client.DownloadString(url);
          }
          catch (WebException ex)
          {
            // A web error occurred (most likely the web site is down).
            var msg = $"An error occurred communicating with the license server. {ex.GetType()}: {ex.Message}";

            if (ex.InnerException != null)
            {
              msg += $" {ex.InnerException.GetType()}: {ex.InnerException.Message}";
            }

            licenseServerResult = new LicenseServerResult() { Reset = false, Error = msg, Code = "901" };
          }
        }

        if (licenseServerResult == null)
        {
          try
          {
            licenseServerResult = JsonConvert.DeserializeObject<LicenseServerResult>(response);
          }
          catch (ArgumentException ex)
          {
            var msg = $"An error occurred communicating with the license server. {ex.GetType()}: {ex.Message} HTTP response: {response}";

            licenseServerResult = new LicenseServerResult() { Reset = false, Error = msg, Code = "902" };
          }
          catch (InvalidCastException ex)
          {
            var msg = $"An error occurred communicating with the license server. {ex.GetType()}: {ex.Message} HTTP response: {response}";

            licenseServerResult = new LicenseServerResult() { Reset = false, Error = msg, Code = "903" };
          }
          catch (JsonReaderException ex)
          {
            var msg = $"An error occurred trying to deserialize a string that was expected to be JSON. {ex.GetType()}: {ex.Message} HTTP response: {response}";

            licenseServerResult = new LicenseServerResult() { Reset = false, Error = msg, Code = "903" };
          }
        }
      }
      else
      {
        // When user originally activated the license, we couldn't reach the license server, but we treated it as a successful activation
        // to minimize user disruption. Since there's nothing to deactivate on the server, just mark it as deactivated and move on.
        licenseServerResult = new LicenseServerResult() { Reset = true };
      }

      if (licenseServerResult.Reset)
      {
        LicenseEmail = string.Empty;
        LicenseKey = string.Empty;
        InstanceId = string.Empty;

        EventController.RecordEvent($"License {LicenseKey} deactivated.", EventType.Info, null, Factory.LoadGallerySettings()); ;
      }
      else
      {
        KeyInvalidReason = $"The license could not be deactivated. {licenseServerResult.Message} {licenseServerResult.Error} (Code {licenseServerResult.Code})";

        var eventMsg = string.Concat(KeyInvalidReason, $". LicenseEmail={LicenseEmail}; LicenseKey={LicenseKey}; InstanceId={InstanceId}; ProductId={LicenseType.GetDescription()}; URL={url}; Server response={response}");
        EventController.RecordEvent(eventMsg, EventType.Error, null, Factory.LoadGallerySettings()); ;
      }

      return licenseServerResult.Reset;
    }

    /// <summary>
    /// Verifies user is conforming to the free version requirements, such as using a DB other than SQL Server. When valid, <see cref="IsValid" /> is set to <c>true</c> 
    /// and the <see cref="LicenseType" /> is set to <see cref="LicenseLevel.Free" />; otherwise <see cref="IsValid" /> is set to <c>false</c> and the 
    /// <see cref="LicenseType" /> is set to <see cref="LicenseLevel.NotSet" />.
    /// </summary>
    private void ValidateFreeLicenseKey()
    {
      if (AppSetting.Instance.ProviderDataStore == ProviderDataStore.SqlServer)
      {
        KeyInvalidReason = "SQL Server detected. The license key you entered requires the use of SQL CE for the data store. You must switch to SQL CE or, if you wish to continue using SQL Server, enter a license key for an edition that supports it. SQL Server offers faster performance and greater reliability. We highly recommend it.";
        IsValid = false;
        LicenseType = LicenseLevel.TrialExpired;
      }
      else
      {
        IsValid = true;
        LicenseType = LicenseLevel.Free;
      }
    }

    /// <summary>
    /// Assigns <see cref="IsValid" /> and <see cref="LicenseType" /> based on whether the trial period has expired.
    /// </summary>
    private void ValidateBlankLicenseKey()
    {
      // No key has been entered, which means we're either in the trial period or it has expired.
      var isInTrialPeriod = (InstallDate.AddDays(GlobalConstants.TrialNumberOfDays) >= DateTime.Today);

      if (isInTrialPeriod)
      {
        IsValid = true;
        LicenseType = LicenseLevel.Trial;
      }
      else
      {
        IsValid = false;
        KeyInvalidReason = "Product key has not been entered and trial period has expired";
        LicenseType = LicenseLevel.TrialExpired;
      }
    }

    /// <summary>
    /// Contact the license server and activate the license. Applies only to commercial versions.
    /// </summary>
    /// <param name="appUrl">The URL to the root of the gallery application.</param>
    private void ActivateLicenseKey(string appUrl)
    {
      if (string.IsNullOrWhiteSpace(LicenseEmail))
      {
        IsValid = false;
        KeyInvalidReason = "Missing e-mail address. Enter the e-mail address used when you purchased the license.";
        return;
      }

      // Call license activation web service
      // API info: https://docs.woothemes.com/document/software-add-on/
      //http://dev.galleryserverpro.com/woocommerce/?wc-api=software-api&request=activation&email=Nixsky84@gmail.com&licence_key=GS-9659000f-f127-45a5-93ac-8d70d99b6adf&product_id=Gallery Server Enterprise&platform=http://localhost/dev/gs/default.aspx
      var url = $"{GlobalConstants.LicenseServerUrl}?wc-api=software-api&request=activation&email={Uri.EscapeDataString(LicenseEmail)}&licence_key={Uri.EscapeDataString(LicenseKey)}&product_id={Uri.EscapeDataString(LicenseType.GetDescription())}&platform={Uri.EscapeDataString(appUrl)}";

      LicenseServerResult licenseServerResult = null;
      string response = null;
      using (var client = new WebClient())
      {
        try
        {
          response = client.DownloadString(url);
        }
        catch (WebException ex)
        {
          // A web error occurred (most likely the web site is down). We don't want activation to be a hassle, so just go ahead and authorize.
          // We'll store a hard-coded instance ID and then detect that if the user later attempts to deactivate to skip going to the server.
          var msg = $"An error occurred communicating with the license server. {ex.GetType()}: {ex.Message}";
          EventController.RecordEvent(msg, EventType.Error, null, Factory.LoadGallerySettings()); ;

          licenseServerResult = new LicenseServerResult() { Activated = true, InstanceId = GlobalConstants.LicenseActivationFailedInstanceId };
        }
      }

      if (licenseServerResult == null)
      {
        if (!string.IsNullOrWhiteSpace(response))
        {
          try
          {
            // We got a string from the server. It should be JSON and convertable to a LicenseServerResult instance.
            licenseServerResult = JsonConvert.DeserializeObject<LicenseServerResult>(response);
          }
          catch (ArgumentException ex)
          {
            var msg = $"An error occurred communicating with the license server. {ex.GetType()}: {ex.Message} HTTP response: {response}";

            licenseServerResult = new LicenseServerResult() { Reset = false, Error = msg, Code = "801" };
          }
          catch (InvalidCastException ex)
          {
            var msg = $"An error occurred communicating with the license server. {ex.GetType()}: {ex.Message} HTTP response: {response}";

            licenseServerResult = new LicenseServerResult() { Reset = false, Error = msg, Code = "802" };
          }
          catch (JsonReaderException ex)
          {
            var msg = $"An error occurred trying to deserialize a string that was expected to be JSON. {ex.GetType()}: {ex.Message} HTTP response: {response}";

            licenseServerResult = new LicenseServerResult() { Reset = false, Error = msg, Code = "903" };
          }
        }
        else
        {
          // If we get here the server call succeeded but we got an empty string.
          licenseServerResult = new LicenseServerResult() { Activated = false, Error = "Server returned an empty response", Code = "803" };
        }
      }

      if (licenseServerResult.Activated)
      {
        InstanceId = licenseServerResult.InstanceId;

        EventController.RecordEvent($"License {LicenseKey} activated.", EventType.Info, null, Factory.LoadGallerySettings()); ;
      }

      IsValid = licenseServerResult.Activated;

      if (!IsValid)
      {
        if (licenseServerResult.Code == "103")
        {
          // User exceeded maximum number of activations.
          KeyInvalidReason = $"The license could not be validated. {licenseServerResult.Message} {licenseServerResult.Error} (Code {licenseServerResult.Code}). Deactivate an existing gallery installation by navigating to the site settings page in that gallery and clicking the deactivate link, then return here and try again. If you need any help, feel free to <a href='https://galleryserverpro.com/get-support/#cntctfrm_contact_form'>contact us</a>.";
        }
        else
        {
          KeyInvalidReason = $"The license could not be validated. {licenseServerResult.Message} {licenseServerResult.Error} (Code {licenseServerResult.Code}). You must enter a license key for {LicenseType.GetDescription()}. If your license key is for another edition, download the version_key.txt file from your <a href='https://galleryserverpro.com/my-account/'>subscriptions</a> page and copy it to the App_Data directory of the web application.";
        }

        var eventMsg = string.Concat(KeyInvalidReason, $" LicenseEmail={LicenseEmail}; LicenseKey={LicenseKey}; ProductId={LicenseType.GetDescription()}; URL={url}; Server response={response}");
        EventController.RecordEvent(eventMsg, EventType.Error, null, Factory.LoadGallerySettings()); ;
      }
    }

    /// <summary>
    /// Validate the version key and assign the <see cref="LicenseType" /> property based on the value from the version key.
    /// If no version key is found or contains invalid data, <see cref="KeyInvalidReason" /> is updated with the error message.
    /// </summary>
    /// <returns><c>true</c> if the current app is running the version found in the version key, <c>false</c> otherwise.</returns>
    /// <remarks></remarks>
    private bool ValidateVersionKey()
    {
      // Check AppSetting.VersionKey or version_key.txt. Verify version matches DLL version.
      var versionKeyStr = RetrieveVersionKeyFromTextFile();

      if (string.IsNullOrWhiteSpace(versionKeyStr))
      {
        versionKeyStr = AppSetting.Instance.VersionKey;

        if (string.IsNullOrWhiteSpace(versionKeyStr))
        {
          KeyInvalidReason = "Missing version_key.txt file. Download this file from your <a href='https://galleryserverpro.com/my-account/'>subscription downloads</a> and copy it to the App_Data directory. ";
          IsValid = false;
          LicenseType = LicenseLevel.NotSet;

          return false;
        }
      }
      else
      {
        // We found version_key.txt. Update our AppSetting and then delete the file.
        lock (_sharedLock)
        {
          AppSetting.Instance.VersionKey = versionKeyStr;
          AppSetting.Instance.Save();
        }

        try
        {
          File.Delete(VersionKeyFilePath);
        }
        catch (Exception ex)
        {
          ex.Data.Add("Info", $"Could not delete the file {VersionKeyFilePath}. Delete it manually.");
          EventController.RecordError(ex, AppSetting.Instance, null, Factory.LoadGallerySettings());
        }
      }

      string versionKeyJson;
      try
      {
        versionKeyJson = Utils.Decrypt(versionKeyStr, GlobalConstants.ENCRYPTION_KEY);
      }
      catch (Exception ex)
      {
        ex.Data.Add("EncryptedVersionKey", versionKeyStr);
        EventController.RecordError(ex, AppSetting.Instance, null, Factory.LoadGallerySettings());
        IsValid = false;
        KeyInvalidReason = "An error occurred while trying to decrypt the version key. The event log contains additional details.";
        return false;
      }

      VersionKey versionKey;
      try
      {
        versionKey = JsonConvert.DeserializeObject<VersionKey>(versionKeyJson);
      }
      catch (Exception ex)
      {
        ex.Data.Add("EncryptedVersionKey", versionKeyStr);
        ex.Data.Add("DecryptedVersionKey", versionKeyJson);
        EventController.RecordError(ex, AppSetting.Instance, null, Factory.LoadGallerySettings());
        IsValid = false;
        KeyInvalidReason = "An error occurred while trying to convert the decrypted version key to an instance of VersionKey. The event log contains additional details.";
        return false;
      }

      var appVersion = GalleryDataSchemaVersionEnumHelper.ConvertGalleryDataSchemaVersionToString(HelperFunctions.GetGalleryServerVersion());

      if (versionKey.Version == appVersion)
      {
        Version = versionKey.Version;
        LicenseType = versionKey.ProductId;

        return true;
      }

      // I don't think we'll typically get here because EF Migrations should clear out the VersionKey, but better safe than sorry.
      KeyInvalidReason = $"You are running version {appVersion} but your license is associated with {versionKey.Version}. Download an updated version_key.txt from your <a href='https://galleryserverpro.com/my-account/'>subscriptions</a> page, copy it to the App_Data directory, and recycle the application pool.";

      return false;
    }

    /// <summary>
    /// If App_Data\version_key.txt exists, parse it for the version key and return. Return null if file does not exist.
    /// </summary>
    /// <returns>System.String.</returns>
    /// <remarks>The parsing routine assumes the version key is in the first line that starts with a value other than --, //, or /*</remarks>
    private string RetrieveVersionKeyFromTextFile()
    {
      if (File.Exists(VersionKeyFilePath))
      {
        try
        {
          using (var sr = new StreamReader(VersionKeyFilePath))
          {
            var lineText = sr.ReadLine();

            while (lineText != null && (string.IsNullOrWhiteSpace(lineText) || lineText.StartsWith("--") || lineText.StartsWith("//") || lineText.StartsWith("/*")))
            {
              lineText = sr.ReadLine();
            }

            return lineText;
          }
        }
        catch (FileNotFoundException) { }
      }

      return null;
    }
  }
}