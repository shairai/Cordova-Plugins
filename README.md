# Adobe® PhoneGap™ Build™ plugin for Facebook Connect

---

## DESCRIPTION

This facebook plugin uses Facebook SDK for .NET and compatible with FacebookConnect plugin [https://github.com/phonegap-build/FacebookConnect]

* Supported on PhoneGap (Cordova) v2.1.0 and above.

## Windows Phone 8

1. [Create a basic Cordova Windows Phone application](http://docs.phonegap.com/en/2.6.0/guide_getting-started_windows-phone-8_index.md.html#Getting%20Started%20with%20Windows%20Phone%208).
 * NOTE: When creating new Windows Phone 8 Cordova Application choose 'Stand-Alone' template.

2. In the Cordova Windows Phone application you will need to put the following in your `res/xml/config.xml` file as a child to the plugin tag: <pre>&lt;plugin name="org.apache.cordova.facebook.Connect" /&gt;</pre>

3. Install the Facebook nuget package into the solution by starting the Package Manager powershell by following:
Tools->Library Package Manager->Package Manager console
Once the powershell command prompt is running, type the following two commands:
<pre>
"Install-Package Facebook"
"Install-Package Facebook.Client -pre"
</pre>

These will download the nuget packages and install the SDK into your project and add it to the references.

4. From the Cordova Facebook Connect Plugin [https://github.com/phonegap-build/FacebookConnect] folder copy the `www/cdv-plugin-fb-connect.js`, `www/facebook-js-sdk.js` into your application's `assets/www` folder. 

5. From the Facebook folder copy the Connect.cs file into you WP8 Cordova project under <pre>cordovalib\Commands</pre> folder.

6. Edit index.html file and add both js files inside the body tag:
<pre>
  &lt;!-- cordova facebook plugin --&gt;
  &lt;script src="cdv-plugin-fb-connect.js"&gt;&lt;/script&gt;
  &lt;!-- facebook js sdk --&gt;
  &lt;script src="facebook-js-sdk.js"&gt;&lt;/script&gt; 
</pre>

7. Finally add the following code below the previous code you add to start Facebook api:
<pre>
<script>
      // Initialize the Facebook SDK
      document.addEventListener('deviceready', function() {
          FB.init({
              appId: 'appid',
              nativeInterface: CDV.FB,
              useCachedDialogs: false
          });
      
          FB.getLoginStatus(function(status)
		  {
			alert(status);
		  });
      
		  FB.login(null, {scope: 'email'});
	  
      });
  </script>
  </pre>
Now you are ready to create your application! Check out the `example` folder for what the HTML, JS etc looks like.

You can run the application from either Visual Studio Emulator or deploy to your Windows Phone 8 device.

