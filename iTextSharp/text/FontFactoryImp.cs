using System.IO;
using System.Collections;
using System.util;
using System.Globalization;

using iTextSharp.text.html;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace iTextSharp.text {
   /// <summary>
   /// If you are using True Type fonts, you can declare the paths of the different ttf- and ttc-files
   /// to this class first and then create fonts in your code using one of the getFont method
   /// without having to enter a path as parameter.
   /// </summary>
   public class FontFactoryImp {

      /// <summary> This is a map of postscriptfontnames of True Type fonts and the path of their ttf- or ttc-file. </summary>
      private Properties trueTypeFonts = new Properties();

      private static string[] TTFamilyOrder = {
         "3", "1", "1033",
         "3", "0", "1033",
         "1", "0", "0",
         "0", "3", "0"
      };

      /// <summary> This is a map of fontfamilies. </summary>
      private Hashtable fontFamilies = new Hashtable();

      /// <summary> This is the default encoding to use. </summary>
      private string defaultEncoding = BaseFont.WINANSI;

      /// <summary> This is the default value of the <VAR>embedded</VAR> variable. </summary>
      private bool defaultEmbedding = BaseFont.NOT_EMBEDDED;

      /// <summary> Creates new FontFactory </summary>
      public FontFactoryImp() {
         trueTypeFonts.Add(FontFactory.COURIER.ToLower(CultureInfo.InvariantCulture), FontFactory.COURIER);
         trueTypeFonts.Add(FontFactory.COURIER_BOLD.ToLower(CultureInfo.InvariantCulture), FontFactory.COURIER_BOLD);
         trueTypeFonts.Add(FontFactory.COURIER_OBLIQUE.ToLower(CultureInfo.InvariantCulture), FontFactory.COURIER_OBLIQUE);
         trueTypeFonts.Add(FontFactory.COURIER_BOLDOBLIQUE.ToLower(CultureInfo.InvariantCulture), FontFactory.COURIER_BOLDOBLIQUE);
         trueTypeFonts.Add(FontFactory.HELVETICA.ToLower(CultureInfo.InvariantCulture), FontFactory.HELVETICA);
         trueTypeFonts.Add(FontFactory.HELVETICA_BOLD.ToLower(CultureInfo.InvariantCulture), FontFactory.HELVETICA_BOLD);
         trueTypeFonts.Add(FontFactory.HELVETICA_OBLIQUE.ToLower(CultureInfo.InvariantCulture), FontFactory.HELVETICA_OBLIQUE);
         trueTypeFonts.Add(FontFactory.HELVETICA_BOLDOBLIQUE.ToLower(CultureInfo.InvariantCulture), FontFactory.HELVETICA_BOLDOBLIQUE);
         trueTypeFonts.Add(FontFactory.SYMBOL.ToLower(CultureInfo.InvariantCulture), FontFactory.SYMBOL);
         trueTypeFonts.Add(FontFactory.TIMES_ROMAN.ToLower(CultureInfo.InvariantCulture), FontFactory.TIMES_ROMAN);
         trueTypeFonts.Add(FontFactory.TIMES_BOLD.ToLower(CultureInfo.InvariantCulture), FontFactory.TIMES_BOLD);
         trueTypeFonts.Add(FontFactory.TIMES_ITALIC.ToLower(CultureInfo.InvariantCulture), FontFactory.TIMES_ITALIC);
         trueTypeFonts.Add(FontFactory.TIMES_BOLDITALIC.ToLower(CultureInfo.InvariantCulture), FontFactory.TIMES_BOLDITALIC);
         trueTypeFonts.Add(FontFactory.ZAPFDINGBATS.ToLower(CultureInfo.InvariantCulture), FontFactory.ZAPFDINGBATS);

         ArrayList tmp;
         tmp = new ArrayList();
         tmp.Add(FontFactory.COURIER);
         tmp.Add(FontFactory.COURIER_BOLD);
         tmp.Add(FontFactory.COURIER_OBLIQUE);
         tmp.Add(FontFactory.COURIER_BOLDOBLIQUE);
         fontFamilies[FontFactory.COURIER.ToLower(CultureInfo.InvariantCulture)] = tmp;
         tmp = new ArrayList();
         tmp.Add(FontFactory.HELVETICA);
         tmp.Add(FontFactory.HELVETICA_BOLD);
         tmp.Add(FontFactory.HELVETICA_OBLIQUE);
         tmp.Add(FontFactory.HELVETICA_BOLDOBLIQUE);
         fontFamilies[FontFactory.HELVETICA.ToLower(CultureInfo.InvariantCulture)] = tmp;
         tmp = new ArrayList();
         tmp.Add(FontFactory.SYMBOL);
         fontFamilies[FontFactory.SYMBOL.ToLower(CultureInfo.InvariantCulture)] = tmp;
         tmp = new ArrayList();
         tmp.Add(FontFactory.TIMES_ROMAN);
         tmp.Add(FontFactory.TIMES_BOLD);
         tmp.Add(FontFactory.TIMES_ITALIC);
         tmp.Add(FontFactory.TIMES_BOLDITALIC);
         fontFamilies[FontFactory.TIMES.ToLower(CultureInfo.InvariantCulture)] = tmp;
         fontFamilies[FontFactory.TIMES_ROMAN.ToLower(CultureInfo.InvariantCulture)] = tmp;
         tmp = new ArrayList();
         tmp.Add(FontFactory.ZAPFDINGBATS);
         fontFamilies[FontFactory.ZAPFDINGBATS.ToLower(CultureInfo.InvariantCulture)] = tmp;
      }

      /// <summary>
      /// Constructs a Font-object.
      /// </summary>
      /// <param name="fontname">the name of the font</param>
      /// <param name="encoding">the encoding of the font</param>
      /// <param name="embedded">true if the font is to be embedded in the PDF</param>
      /// <param name="size">the size of this font</param>
      /// <param name="style">the style of this font</param>
      /// <param name="color">the Color of this font</param>
      /// <returns>a Font object</returns>
      public virtual Font GetFont(string fontname, string encoding, bool embedded, float size, int style, Color color) {
         return GetFont(fontname, encoding, embedded, size, style, color, true);
      }

      /**
      * Constructs a Font-object.
      *
      * @param fontname the name of the fontparam
      * @param encoding the encoding of the font
      * @param embedded true if the font is to be embedded in the PDF
      * @param size the size of this font
      * @param style the style of this font
      * @param color the Color of this font
      * @param cached true if the font comes from the cache or is added to the cache if new, false if the font is always created new
      * @returns a Font object
      */
      public virtual Font GetFont(string fontname, string encoding, bool embedded, float size, int style, Color color, bool cached) {
         if (fontname == null) return new Font(Font.UNDEFINED, size, style, color);
         string lowercasefontname = fontname.ToLower(CultureInfo.InvariantCulture);
         ArrayList tmp = (ArrayList) fontFamilies[lowercasefontname];
         if (tmp != null) {
            // some bugs were fixed here by Daniel Marczisovszky
            int fs = Font.NORMAL;
            bool found = false;
            int s = style == Font.UNDEFINED ? Font.NORMAL : style;
            foreach (string f in tmp) {
               string lcf = f.ToLower(CultureInfo.InvariantCulture);
               fs = Font.NORMAL;
               if (lcf.ToLower(CultureInfo.InvariantCulture).IndexOf("bold") != -1) fs |= Font.BOLD;
               if (lcf.ToLower(CultureInfo.InvariantCulture).IndexOf("italic") != -1 || lcf.ToLower(CultureInfo.InvariantCulture).IndexOf("oblique") != -1) fs |= Font.ITALIC;
               if ((s & Font.BOLDITALIC) == fs) {
                  fontname = f;
                  found = true;
                  break;
               }
            }
            if (style != Font.UNDEFINED && found) {
               style &= ~fs;
            }
         }
         BaseFont basefont = null;
         try {
            try {
               // the font is a type 1 font or CJK font
               basefont = BaseFont.CreateFont(fontname, encoding, embedded, cached, null, null, true);
            }
            catch (DocumentException) {
            }
            if (basefont == null) {
               // the font is a true type font or an unknown font
               fontname = trueTypeFonts[fontname.ToLower(CultureInfo.InvariantCulture)];
               // the font is not registered as truetype font
               if (fontname == null) return new Font(Font.UNDEFINED, size, style, color);
               // the font is registered as truetype font
               basefont = BaseFont.CreateFont(fontname, encoding, embedded, cached, null, null);
            }
         }
         catch (DocumentException de) {
            // this shouldn't happen
            throw de;
         }
         catch (System.IO.IOException) {
            // the font is registered as a true type font, but the path was wrong
            return new Font(Font.UNDEFINED, size, style, color);
         }
         catch {
            // null was entered as fontname and/or encoding
            return new Font(Font.UNDEFINED, size, style, color);
         }
         return new Font(basefont, size, style, color);
      }

      /// <summary>
      /// Constructs a Font-object.
      /// </summary>
      /// <param name="attributes">the attributes of a Font object</param>
      /// <returns>a Font object</returns>
      public virtual Font GetFont(Properties attributes) {
         string fontname = null;
         string encoding = defaultEncoding;
         bool embedded = defaultEmbedding;
         float size = Font.UNDEFINED;
         int style = Font.NORMAL;
         Color color = null;
         string value = attributes[Markup.HTML_ATTR_STYLE];
         if (value != null && value.Length > 0) {
            Properties styleAttributes = Markup.ParseAttributes(value);
            if (styleAttributes.Count == 0) {
               attributes.Add(Markup.HTML_ATTR_STYLE, value);
            }
            else {
               fontname = styleAttributes[Markup.CSS_KEY_FONTFAMILY];
               if (fontname != null) {
                  string tmp;
                  while (fontname.IndexOf(',') != -1) {
                     tmp = fontname.Substring(0, fontname.IndexOf(','));
                     if (IsRegistered(tmp)) {
                        fontname = tmp;
                     }
                     else {
                        fontname = fontname.Substring(fontname.IndexOf(',') + 1);
                     }
                  }
               }
               if ((value = styleAttributes[Markup.CSS_KEY_FONTSIZE]) != null) {
                  size = Markup.ParseLength(value);
               }
               if ((value = styleAttributes[Markup.CSS_KEY_FONTWEIGHT]) != null) {
                  style |= Font.GetStyleValue(value);
               }
               if ((value = styleAttributes[Markup.CSS_KEY_FONTSTYLE]) != null) {
                  style |= Font.GetStyleValue(value);
               }
               if ((value = styleAttributes[Markup.CSS_KEY_COLOR]) != null) {
                  color = Markup.DecodeColor(value);
               }
               attributes.AddAll(styleAttributes);
            }
         }
         if ((value = attributes[ElementTags.ENCODING]) != null) {
            encoding = value;
         }
         if ("true".Equals(attributes[ElementTags.EMBEDDED])) {
            embedded = true;
         }
         if ((value = attributes[ElementTags.FONT]) != null) {
            fontname = value;
         }
         if ((value = attributes[ElementTags.SIZE]) != null) {
            size = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
         }
         if ((value = attributes[Markup.HTML_ATTR_STYLE]) != null) {
            style |= Font.GetStyleValue(value);
         }
         if ((value = attributes[ElementTags.STYLE]) != null) {
            style |= Font.GetStyleValue(value);
         }
         string r = attributes[ElementTags.RED];
         string g = attributes[ElementTags.GREEN];
         string b = attributes[ElementTags.BLUE];
         if (r != null || g != null || b != null) {
            int red = 0;
            int green = 0;
            int blue = 0;
            if (r != null) red = int.Parse(r);
            if (g != null) green = int.Parse(g);
            if (b != null) blue = int.Parse(b);
            color = new Color(red, green, blue);
         }
         else if ((value = attributes[ElementTags.COLOR]) != null) {
            color = Markup.DecodeColor(value);
         }
         if (fontname == null) {
            return GetFont(null, encoding, embedded, size, style, color);
         }
         return GetFont(fontname, encoding, embedded, size, style, color);
      }

      /// <summary>
      /// Constructs a Font-object.
      /// </summary>
      /// <param name="fontname">the name of the font</param>
      /// <param name="encoding">the encoding of the font</param>
      /// <param name="embedded">true if the font is to be embedded in the PDF</param>
      /// <param name="size">the size of this font</param>
      /// <param name="style">the style of this font</param>
      /// <returns>a Font object</returns>
      public Font GetFont(string fontname, string encoding, bool embedded, float size, int style) {
         return GetFont(fontname, encoding, embedded, size, style, null);
      }

      /// <summary>
      /// Constructs a Font-object.
      /// </summary>
      /// <param name="fontname">the name of the font</param>
      /// <param name="encoding">the encoding of the font</param>
      /// <param name="embedded">true if the font is to be embedded in the PDF</param>
      /// <param name="size">the size of this font</param>
      /// <returns></returns>
      public virtual Font GetFont(string fontname, string encoding, bool embedded, float size) {
         return GetFont(fontname, encoding, embedded, size, Font.UNDEFINED, null);
      }

      /// <summary>
      /// Constructs a Font-object.
      /// </summary>
      /// <param name="fontname">the name of the font</param>
      /// <param name="encoding">the encoding of the font</param>
      /// <param name="embedded">true if the font is to be embedded in the PDF</param>
      /// <returns>a Font object</returns>
      public virtual Font GetFont(string fontname, string encoding, bool embedded) {
         return GetFont(fontname, encoding, embedded, Font.UNDEFINED, Font.UNDEFINED, null);
      }

      /// <summary>
      /// Constructs a Font-object.
      /// </summary>
      /// <param name="fontname">the name of the font</param>
      /// <param name="encoding">the encoding of the font</param>
      /// <param name="size">the size of this font</param>
      /// <param name="style">the style of this font</param>
      /// <param name="color">the Color of this font</param>
      /// <returns>a Font object</returns>
      public virtual Font GetFont(string fontname, string encoding, float size, int style, Color color) {
         return GetFont(fontname, encoding, defaultEmbedding, size, style, color);
      }

      /// <summary>
      /// Constructs a Font-object.
      /// </summary>
      /// <param name="fontname">the name of the font</param>
      /// <param name="encoding">the encoding of the font</param>
      /// <param name="size">the size of this font</param>
      /// <param name="style">the style of this font</param>
      /// <returns>a Font object</returns>
      public virtual Font GetFont(string fontname, string encoding, float size, int style) {
         return GetFont(fontname, encoding, defaultEmbedding, size, style, null);
      }

      /// <summary>
      /// Constructs a Font-object.
      /// </summary>
      /// <param name="fontname">the name of the font</param>
      /// <param name="encoding">the encoding of the font</param>
      /// <param name="size">the size of this font</param>
      /// <returns>a Font object</returns>
      public virtual Font GetFont(string fontname, string encoding, float size) {
         return GetFont(fontname, encoding, defaultEmbedding, size, Font.UNDEFINED, null);
      }

      /// <summary>
      /// Constructs a Font-object.
      /// </summary>
      /// <param name="fontname">the name of the font</param>
      /// <param name="encoding">the encoding of the font</param>
      /// <returns>a Font object</returns>
      public virtual Font GetFont(string fontname, string encoding) {
         return GetFont(fontname, encoding, defaultEmbedding, Font.UNDEFINED, Font.UNDEFINED, null);
      }

      /// <summary>
      /// Constructs a Font-object.
      /// </summary>
      /// <param name="fontname">the name of the font</param>
      /// <param name="size">the size of this font</param>
      /// <param name="style">the style of this font</param>
      /// <param name="color">the Color of this font</param>
      /// <returns>a Font object</returns>
      public virtual Font GetFont(string fontname, float size, int style, Color color) {
         return GetFont(fontname, defaultEncoding, defaultEmbedding, size, style, color);
      }

      /// <summary>
      /// Constructs a Font-object.
      /// </summary>
      /// <param name="fontname">the name of the font</param>
      /// <param name="size">the size of this font</param>
      /// <param name="color">the Color of this font</param>
      /// <returns>a Font object</returns>
      public virtual Font GetFont(string fontname, float size, Color color) {
         return GetFont(fontname, defaultEncoding, defaultEmbedding, size, Font.UNDEFINED, color);
      }

      /// <summary>
      /// Constructs a Font-object.
      /// </summary>
      /// <param name="fontname">the name of the font</param>
      /// <param name="size">the size of this font</param>
      /// <param name="style">the style of this font</param>
      /// <returns>a Font object</returns>
      public virtual Font GetFont(string fontname, float size, int style) {
         return GetFont(fontname, defaultEncoding, defaultEmbedding, size, style, null);
      }

      /// <summary>
      /// Constructs a Font-object.
      /// </summary>
      /// <param name="fontname">the name of the font</param>
      /// <param name="size">the size of this font</param>
      /// <returns>a Font object</returns>
      public virtual Font GetFont(string fontname, float size) {
         return GetFont(fontname, defaultEncoding, defaultEmbedding, size, Font.UNDEFINED, null);
      }

      /// <summary>
      /// Constructs a Font-object.
      /// </summary>
      /// <param name="fontname">the name of the font</param>
      /// <returns>a Font object</returns>
      public virtual Font GetFont(string fontname) {
         return GetFont(fontname, defaultEncoding, defaultEmbedding, Font.UNDEFINED, Font.UNDEFINED, null);
      }

      public virtual void Register(Properties attributes) {
         string path;
         string alias = null;

         path = attributes.Remove("path");
         alias = attributes.Remove("alias");

         Register(path, alias);
      }

      /**
         Register a font by giving explicitly the font family and name.
         @param familyName the font family
         @param fullName the font name
         @param path the font path
      */
      public void RegisterFamily(string familyName, string fullName, string path) {
         if (path != null) {
            trueTypeFonts.Add(fullName, path);
         }
         ArrayList tmp = (ArrayList) fontFamilies[familyName];
         if (tmp == null) {
            tmp = new ArrayList();
            tmp.Add(fullName);
            fontFamilies[familyName] = tmp;
         }
         else {
            int fullNameLength = fullName.Length;
            bool inserted = false;
            for (int j = 0; j < tmp.Count; ++j) {
               if (((string)tmp[j]).Length >= fullNameLength) {
                  tmp.Insert(j, fullName);
                  inserted = true;
                  break;
               }
            }
            if (!inserted)
               tmp.Add(fullName);
         }
      }

      /// <summary>
      /// Register a ttf- or a ttc-file.
      /// </summary>
      /// <param name="path">the path to a ttf- or ttc-file</param>
      public virtual void Register(string path) {
         Register(path, null);
      }

      /**
       * Register a ttf- or a ttc-file and use an alias for the font contained in the ttf-file.
       *
       * @parm path the path to a ttf- or ttc-file
       * @parm alias the alias you want to use for the font
      */
      public virtual void Register(string path, string alias) {
         try {
            if (path.ToLower(CultureInfo.InvariantCulture).EndsWith(".ttf") || path.ToLower(CultureInfo.InvariantCulture).EndsWith(".otf") || path.ToLower(CultureInfo.InvariantCulture).IndexOf(".ttc,") > 0) {
               object[] allNames = BaseFont.GetAllFontNames(path, BaseFont.WINANSI, null);
               trueTypeFonts.Add(((string)allNames[0]).ToLower(CultureInfo.InvariantCulture), path);
               if (!string.IsNullOrEmpty(alias)) {
                  trueTypeFonts.Add(alias.ToLower(CultureInfo.InvariantCulture), path);
               }
               // register all the font names with all the locales
               string[][] names = (string[][])allNames[2]; //full name
               for (int i = 0; i < names.Length; i++) {
                  trueTypeFonts.Add(names[i][3].ToLower(CultureInfo.InvariantCulture), path);
               }
               string fullName = null;
               string familyName = null;
               names = (string[][])allNames[1]; //family name
               for (int k = 0; k < TTFamilyOrder.Length; k += 3) {
                  for (int i = 0; i < names.Length; i++) {
                     if (TTFamilyOrder[k].Equals(names[i][0]) && TTFamilyOrder[k + 1].Equals(names[i][1]) && TTFamilyOrder[k + 2].Equals(names[i][2])) {
                        familyName = names[i][3].ToLower(CultureInfo.InvariantCulture);
                        k = TTFamilyOrder.Length;
                        break;
                     }
                  }
               }
               if (familyName != null) {
                  string lastName = "";
                  names = (string[][])allNames[2]; //full name
                  for (int i = 0; i < names.Length; i++) {
                     for (int k = 0; k < TTFamilyOrder.Length; k += 3) {
                        if (TTFamilyOrder[k].Equals(names[i][0]) && TTFamilyOrder[k + 1].Equals(names[i][1]) && TTFamilyOrder[k + 2].Equals(names[i][2])) {
                           fullName = names[i][3];
                           if (fullName.Equals(lastName))
                              continue;
                           lastName = fullName;
                           RegisterFamily(familyName, fullName, null);
                           break;
                        }
                     }
                  }
               }
            } else if (path.ToLower(CultureInfo.InvariantCulture).EndsWith(".ttc")) {
               string[] names = BaseFont.EnumerateTTCNames(path);
               for (int i = 0; i < names.Length; i++) {
                  Register(path + "," + i);
               }
            } else if (path.ToLower(CultureInfo.InvariantCulture).EndsWith(".afm") || path.ToLower(CultureInfo.InvariantCulture).EndsWith(".pfm")) {
               BaseFont bf = BaseFont.CreateFont(path, BaseFont.CP1252, false);
               string fullName = (bf.FullFontName[0][3]).ToLower(CultureInfo.InvariantCulture);
               string familyName = (bf.FamilyFontName[0][3]).ToLower(CultureInfo.InvariantCulture);
               string psName = bf.PostscriptFontName.ToLower(CultureInfo.InvariantCulture);
               RegisterFamily(familyName, fullName, null);
               trueTypeFonts.Add(psName, path);
               trueTypeFonts.Add(fullName, path);
            }
         } catch (DocumentException de) {
            // this shouldn't happen
            throw de;
         } catch (System.IO.IOException ioe) {
            throw ioe;
         }
      }

      /**
       * Register all the fonts in a directory.
       * @param dir the directory
       * @return the number of fonts registered
       */
      public virtual int RegisterDirectory(string dir) {
         return RegisterDirectory(dir, false);
      }

      /**
       * Register all the fonts in a directory and possibly its subdirectories.
       * @param dir the directory
       * @param scanSubdirectories recursively scan subdirectories if <code>true</true>
       * @return the number of fonts registered
       * @since 2.1.2
       */
      public int RegisterDirectory(string baseDir, bool scanSubdirectories) {
         int count = 0;
         try {
            if (!Directory.Exists(baseDir)) return 0;

            string[] files = Directory.GetFiles(baseDir);
            if (files != null) {
               foreach (string file in files) {
                  string name = Path.GetFullPath(file);
                  string suffix = GetSuffix(name);
                  if (".afm".Equals(suffix) || ".pfm".Equals(suffix)) {
                     /* Only register Type 1 fonts with matching .pfb files */
                     string pfb = name.Substring(0, name.Length - 4) + ".pfb";
                     if (File.Exists(pfb)) {
                        Register(name, null);
                        ++count;
                     }
                  } else if (".ttf".Equals(suffix) || ".otf".Equals(suffix) || ".ttc".Equals(suffix)) {
                     Register(name, null);
                     ++count;
                  }
               }
            }
            if (scanSubdirectories) {
               var dirs = System.IO.Directory.GetDirectories(baseDir);
               foreach (string dir in dirs) {
                  count += RegisterDirectory(dir, true);
               }
            }
         } catch  {
            //empty on purpose
         }
         return count;
      }

      private string GetSuffix(string name) {
         return name.Length < 4 ? null : name.Substring(name.Length - 4).ToLower(CultureInfo.InvariantCulture);
      }

      /**
          Register fonts in some probable directories. It usually works in Windows,
          Linux and Solaris.
          @return the number of fonts registered
      */
      public virtual int RegisterDirectories() {
         int count = 0;
         count += RegisterDirectory("c:/windows/fonts");
         count += RegisterDirectory("c:/winnt/fonts");
         count += RegisterDirectory("d:/windows/fonts");
         count += RegisterDirectory("d:/winnt/fonts");
         count += RegisterDirectory("/usr/share/X11/fonts", true);
         count += RegisterDirectory("/usr/X/lib/X11/fonts", true);
         count += RegisterDirectory("/usr/openwin/lib/X11/fonts", true);
         count += RegisterDirectory("/usr/share/fonts", true);
         count += RegisterDirectory("/usr/X11R6/lib/X11/fonts", true);
         count += RegisterDirectory("/Library/Fonts");
         count += RegisterDirectory("/System/Library/Fonts");
         return count;
      }

      /**
       * Gets a set of registered fontnames.
      */
      public virtual ICollection RegisteredFonts {
         get {
            return trueTypeFonts.Keys;
         }
      }

      /// <summary>
      /// Gets a set of registered font families.
      /// </summary>
      /// <value>a set of registered font families</value>
      public virtual ICollection RegisteredFamilies {
         get {
            return fontFamilies.Keys;
         }
      }

      /// <summary>
      /// Checks if a certain font is registered.
      /// </summary>
      /// <param name="fontname">the name of the font that has to be checked</param>
      /// <returns>true if the font is found</returns>
      public virtual bool IsRegistered(string fontname) {
         return trueTypeFonts.ContainsKey(fontname.ToLower(CultureInfo.InvariantCulture));
      }

      public virtual string DefaultEncoding {
         get {
            return defaultEncoding;
         }
         set {
            defaultEncoding = value;
         }
      }

      public virtual bool DefaultEmbedding {
         get {
            return defaultEmbedding;
         }
         set {
            defaultEmbedding = value;
         }
      }
   }
}
