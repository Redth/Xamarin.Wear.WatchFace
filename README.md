Xamarin.Wear.WatchFace
======================

An Android Wear watch face made with Xamarin.

NOTE: This project requires a  Xamarin.Android build supporting 4.4.87 and/or L Preview

## Permissions
Your app must have the permission:
`com.google.android.permission.PROVIDE_BACKGROUND`


## Watch Activity AndroidManifest.xml
Your watch activity needs to have its manifest entry created in a specific way. It should specify `android:allowEmbedded="true"`, an empty task affinity (`android:taskAffinity=""`), and needs to have a special `metadata` tag inside the `activity` tag like below, which points to a 320x320 pixel drawable image which will be displayed in the watch face selection screen to the user.  Finally, it most *ONLY* have the Intent Filter action and category as displayed below:

```
<activity
    android:theme="@android:style/Theme.DeviceDefault.NoActionBar"
    android:taskAffinity=""
    android:allowEmbedded="true" >
 
    <meta-data android:name="com.google.android.clockwork.home.preview" android:resource="@drawable/preview" />
 
    <intent-filter>
        <action android:name="android.intent.action.MAIN" />
        <category android:name="com.google.android.clockwork.home.category.HOME_BACKGROUND" />
    </intent-filter>
</activity>
```


