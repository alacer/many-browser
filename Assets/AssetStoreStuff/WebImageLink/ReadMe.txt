WebImageLinks: Unity script
(c) Copyright 2013 Twistplay Ltd

Please open the example scene in the Scenes folder for sample usage.

 
Grabs an image and a hyperlink from a website and displays it on a GUI button, e.g. for more dynamic GUIs, links to external sites and in-game advertising.
Will cache the image on local storage rather than downloading it from the web on each load. The web page loading is performed asynchronously, so on first
load there may be a delay before the image appears.
 


Typical usage: 

Add the WebImageLink component to a game object, and in "Containing page general" set the URL for the webpage that contains the reference to your image.
The URL is in standard web format and should include http:// or https:// at the start, e.g. "http://www.unity3d.com/unity/".


You can set "Containing Page IOS" to something different if you want to use a different image/link on iOS platforms - if not be sure to set it to the same value
as "Containing Page"

Optionally you can use different pages for different stores/platforms on Android (e.g. Amazon, Google Play), to do so use the scripting define symbols in
Build Settings, and define either "STORE_GOOGLE" or "STORE_AMAZON" (with underscores between the words).  If these are not defined it will revert back to the
value in "Containing Page General"

Generally the "Containing Page" should be a web page that you control, that has a reference to the image you want to use.  
You can also set the "Containing Page" to a URL for the image itself - this is determined by a URL that ends with a .jpg or .png extension.   In this case you
should manually set the "URL To Open" fields as there is no webpage to parse in order to extract the hyperlink.



Uses Unity's in-built GUI system via the OnGUI function, but feel free to disable that method and just use the image reference directly.   Note it is
perfectly valid for the image to be null, e.g. if no internet connection is found or on startup the web page is still being loaded in the background.   You may
assign a default image in Unity in the inspector if you wish, which will get overwritten only once an image has been successfuully loaded from the web.

Limitations:

- Won't parse through CSS or Javascript to find images, just looks for <img tags
- This is not a full HTML parser, so won't correctly recurse into nested tags
- Only extracts 1 image reference from the containing page
- For the link to open when clicked, uses most recent hyperlink before the <img tag, so this may not always be correct 
- Only the image formats supported by Unity's built in WWW class can be used, i.e. PNG or JPG, so no GIFs I'm sorry to say


Any support queries to support@twistplay.com
