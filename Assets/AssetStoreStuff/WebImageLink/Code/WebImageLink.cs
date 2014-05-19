/*
 * Copyright 2013 Twistplay Ltd
 * 
 * Grabs an image and a hyperlink from a website and displays it on a GUI button, e.g. for more dynamic GUIs, links to external sites and in-game advertising
 * Will cache the image on local storage rather than downloading it from the web on each load.     The web page loading is performed asynchronously, so on first load there may be a delay
 * before the image appears.
 * 
 * Add this component to a game object, set the root URL for the website where your image is contained, and set which page on that site contains the reference to your image
 * 
 * You can set containingPageIOS to something different if you want to use a different image/link on iOS platforms - if not be sure to set it to the same value as containingPage
 * 
 * Optionally you can use different pages for different stores/platforms on Android (e.g. Amazon, Google Play), to do so use the scripting define symbols in Build Settings, and define either "STORE_GOOGLE" or "STORE_AMAZON" (with underscores between the words)
 * 
 * 
 * Generally the "containing page" should be a web page that you control, that has a reference to the image you want to use.  
 * 
 * 
 * Uses Unity's in-built GUI system via the OnGUI function, but feel free to disable that method and just use the image reference directly.   Note it is perfectly valid for the image
 * to be null, e.g. if no internet connection is found or on startup the web page is still being loaded in the background.   You may assign a default image in Unity in the inspector if you wish,
 * which will get overwritten only once an image has been successfuully loaded from the web.
 * 
 * Limitations:
 * 
 * - Won't parse through CSS or Javascript to find images, just looks for <img tags
 * - This is not a full HTML parser, so won't correctly recurse into nested tags
 * - Only extracts 1 image reference from the containing page
 * - For the link to open when clicked, uses most recent hyperlink before the <img tag, so this may not always be correct 
 * - Only the image formats supported by Unity's built in WWW class can be used, i.e. PNG or JPG, so no GIFs I'm sorry to say
 * - Deliberately doesn't support 16x16 images and smaller, as these are assumed to be Unity's default ? image when no image can be successfully loaded
 */

using UnityEngine;
using System.Collections;
using System.IO;

public class WebImageLink : MonoBehaviour
{
	public Texture2D image = null;
	public int cacheTimeoutInHours = 4;
	
	public Vector2 position;
	
	public string containingPageGeneral = "http://www.unity3d.com/unity/";
    public string containingPageGooglePlay = "http://www.unity3d.com/unity/";
    public string containingPageAmazon = "http://www.unity3d.com/unity/";
    public string containingPageIOS = "http://www.unity3d.com/unity/"; 
	
	/// <summary>
	/// Default URLs to open if one can't be extracted from the containing page
	/// </summary>
	public string urlToOpenGeneral = "";
	public string urlToOpenGooglePlay = "";
	public string urlToOpenAmazon = "";
	public string urlToOpenIOS = "";
	
	public enum Sizing
	{
		FixedSize,
		AspectCorrectFixedWidth,
		AspectCorrectFixedHeight,
		PixelSizeFromSourceImage
	}
	
	
	/// <summary>
	/// How the size image will be scaled
	/// </summary>
	public Sizing imageScaling;

	/// <summary>
	/// Dimensions to use if imageScaling is FixedSize
	/// </summary>
	public Vector2 fixedSize = new Vector2 (100,100);

	
	private Rect displayBounds = new Rect (0,0,400,200);	
	private string urlToOpen = "";
	private WWW wwwImage = null;
	private WWW wwwContainingPage = null;
	
	// a unique image id is needed if we want to cache multiple images correctly
	// if using multiple images, to avoid hash collisions please name the gameObject something unique	
	// deliberately not using GetInstanceID as it is not guaranteed to be the same between multiple runs
	string GetImageID ()
	{
		return name.ToString() + "-" + GetContainingURL().GetHashCode().ToString();
	}
	
	string GetCacheFilePath ()
	{
		return Application.temporaryCachePath + ((Application.temporaryCachePath.Length > 0) ? "/" : "") + GetImageID() + ".png";
	}

	string GetContainingURLFilePath ()
	{
		return Application.temporaryCachePath + ((Application.temporaryCachePath.Length > 0) ? "/" : "") + GetImageID() + "ContainingURL.txt";
	}

	string GetURLToOpenFilePath ()
	{
		return Application.temporaryCachePath + ((Application.temporaryCachePath.Length > 0) ? "/" : "") + GetImageID() + "URL.txt";
	}
	
	// return full URL of the page containing the image
	string GetContainingURL ()
	{
#if (UNITY_IOS || UNITY_IPHONE)
		return containingPageIOS;
#elif STORE_GOOGLE
		return containingPageGooglePlay;
#elif STORE_AMAZON
		return containingPageAmazon;
#else
		return containingPageGeneral;
#endif		
	}
	
	void BeginRefreshImage ()
	{
        string url = GetContainingURL();
   		wwwContainingPage = new WWW (url);
	}
	
	void LoadImageFromCache ()
	{
		if (File.Exists (GetCacheFilePath()))
		{
			byte[] png = File.ReadAllBytes (GetCacheFilePath());
			Texture2D pngImage = new Texture2D (16,16);
			if (pngImage.LoadImage (png))
			{
				image = pngImage;
				image.wrapMode = TextureWrapMode.Clamp;
			}
			if (File.Exists (GetURLToOpenFilePath ()))
			{
				urlToOpen = File.ReadAllText (GetURLToOpenFilePath());
			}
		}
	}
	
	bool IsCacheOld ()
	{
		string filePath = GetCacheFilePath ();
		
		if (File.Exists (GetContainingURLFilePath ()))
		{
			if (GetContainingURL() != File.ReadAllText (GetContainingURLFilePath()))
			{
				// URL containing the image has changed (possibly caused by a hash collision in GetImageID)
				return true;
			}
		}
		else
		{
			return true;
		}
		
		if (File.Exists (filePath))
		{
			if (System.DateTime.Now.Subtract (File.GetLastWriteTime (filePath)).Hours < cacheTimeoutInHours)
			{
				return false;
			}
		}
		


		return true;
	}
				

	
	void CheckImage ()
	{
		LoadImageFromCache ();

		if (IsCacheOld ())
		{
			BeginRefreshImage ();
		}
				
	}
	
	void Awake ()
	{
#if (UNITY_IOS || UNITY_IPHONE)
		urlToOpen = urlToOpenIOS;
#elif STORE_GOOGLE
		urlToOpen = urlToOpenGooglePlay;
#elif STORE_AMAZON
		urlToOpen = urlToOpenAmazon;
#else
		urlToOpen = urlToOpenGeneral;
#endif		
	}
	
	void Start ()
	{
		CheckImage ();
		if (image != null)
		{
			SetBounds ();
		}
	}
	
	string FindQuotedText (string page,int startLocation)
	{
		int quoteLocation = page.IndexOfAny (new char[] {'\'','\"'},startLocation);
		if (quoteLocation != -1)
		{
			int quoteCloseLocation = page.IndexOfAny (new char[] {'\'','\"'},quoteLocation+1);
			if (quoteCloseLocation != -1)
			{
				return page.Substring (quoteLocation+1,quoteCloseLocation-quoteLocation - 1);
			}
		}
		return "";
	}
	
	// search for the next occurence of given attribute after tag, and return quoted text associated with it 
	string FindTagAttribute(string page,string tag,string attribute,ref int searchLocation)
	{
		int tagLocation = page.IndexOf (tag,searchLocation);
		if (tagLocation != -1)
		{
			searchLocation = tagLocation;
			int attrLocation = page.IndexOf (attribute,tagLocation);
			if (attrLocation != -1)
			{
				return FindQuotedText (page, attrLocation);
			}
			else
			{
				return "";
			}
		}
		
		// none found
		return null;
	}
	
	void SetBounds ()
	{
		switch (imageScaling)
		{
			case Sizing.FixedSize:
				displayBounds = new Rect (position.x,position.y,fixedSize.x,fixedSize.y);
				break;
			case Sizing.PixelSizeFromSourceImage:
				displayBounds = new Rect (position.x,position.y,image.width,image.height);
				break;			
			case Sizing.AspectCorrectFixedWidth:
				displayBounds = new Rect (position.x,position.y,fixedSize.x,fixedSize.x * image.height / image.width);
				break;
			case Sizing.AspectCorrectFixedHeight:
				displayBounds = new Rect (position.x,position.y,fixedSize.y * image.width / image.height,fixedSize.y);
				break;
		}
	}
	
	void Update ()
	{
		if ((wwwContainingPage != null) && (wwwContainingPage.isDone))
		{
			if ((wwwContainingPage.error == null) || (wwwContainingPage.error.Length == 0))
			{
                string url = GetContainingURL().ToLower();
                if (url.EndsWith(".jpg") || url.EndsWith(".jpeg") || url.EndsWith(".png"))
                {
                    if ((wwwContainingPage.texture != null) && (wwwContainingPage.texture.width > 16))
                    {
                        // direct reference to an image
                        wwwImage = wwwContainingPage;
                        wwwContainingPage = null;
                    }
                }

                if (wwwContainingPage != null)
                {
                    string page = wwwContainingPage.text;
                    bool imgFound = false;
                    string imgAddress = "";
                    int imgLocation = 0;
                    while (!imgFound && imgAddress != null)
                    {
                        imgAddress = FindTagAttribute(page, "<img", "src", ref imgLocation);
                        imgLocation++;
                        if (imgAddress != null)
                        {
                            if ((imgAddress.Length > 0) && !imgAddress.ToLower().EndsWith(".gif"))
                            {
                                imgFound = true;
                            }
                        }
                    }
                    if (imgFound)
                    {
                        System.Uri imageUri;
                        if (System.Uri.TryCreate(imgAddress, System.UriKind.RelativeOrAbsolute, out imageUri) && imageUri.IsAbsoluteUri)
                        {
                            wwwImage = new WWW(imgAddress);
                        }
                        else
                        {
                            wwwImage = new WWW(new System.Uri(new System.Uri(GetContainingURL()), imgAddress).AbsoluteUri);
                        }
                        // search backwards for most recent <a tag
                        int aLocation = page.LastIndexOf("<a ", imgLocation, imgLocation);
                        if (aLocation == -1)
                        {
                            // search forwards instead
                            aLocation = page.IndexOf("<a ", imgLocation);
                            if (aLocation == -1)
                            {
                                // search backwards for most recent <area tag	 instead			
                                aLocation = page.LastIndexOf("<area ", imgLocation, imgLocation);
                            }
                        }
                        if (aLocation != -1)
                        {
                            int hrefLocation = page.IndexOf("href", aLocation);
                            if (hrefLocation != -1)
                            {
                                string linkAddress = FindQuotedText(page, hrefLocation);
                                if (linkAddress == "#")
                                {
                                    // open the page that contained the image link
                                    urlToOpen = GetContainingURL();
                                }
                                else if (linkAddress.Length > 0)
                                {
                                    System.Uri uri;
                                    if (System.Uri.TryCreate(linkAddress, System.UriKind.RelativeOrAbsolute, out uri) && uri.IsAbsoluteUri)
                                    {
                                        urlToOpen = linkAddress;
                                    }
                                    else
                                    {
                                        urlToOpen = new System.Uri(new System.Uri(GetContainingURL()), linkAddress).AbsoluteUri;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("No image reference found in containing page");
                    }
                }
			}
			else
			{
				Debug.Log ("Error opening containing page: " + wwwContainingPage.error);
			}
			wwwContainingPage = null;
		}

		if ((wwwImage != null) && (wwwImage.isDone))
		{
			if ((wwwImage.error == null) || (wwwImage.error.Length == 0))
			{
				Texture2D pngImage = wwwImage.texture;
				// reject small textures, probably the question mark default
				if (pngImage.width > 16)
				{
					image = pngImage;
					SetBounds ();
					image.wrapMode = TextureWrapMode.Clamp;
					byte[] png = pngImage.EncodeToPNG ();
					File.WriteAllBytes (GetCacheFilePath(),png);
					File.WriteAllText (GetURLToOpenFilePath(),urlToOpen);
					File.WriteAllText (GetContainingURLFilePath(),GetContainingURL());
				}
				else	
				{
					Debug.Log ("Invalid image detected at URL " + wwwImage.url);
				}
			}
			else
			{
				Debug.Log ("Error opening image " + wwwImage.error);
			}
			wwwImage = null;
		}
	}
	
	public void DrawInGUI (Rect r)
	{
		if (image != null)
		{
			GUI.DrawTexture (r,image);
			GUIStyle bstyle = new GUIStyle (GUI.skin.button);
			bstyle.normal.background = null;
			bstyle.active.background = null;
			bstyle.hover.background = null;
			bstyle.focused.background = null;
			if (GUI.Button (r,"",bstyle))
			{
				Application.OpenURL (urlToOpen);
			}
		}
	}
	
	public void OnGUI ()
	{
		DrawInGUI (displayBounds);
	}
	
}

