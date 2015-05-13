using Dapplo.Config.Ini;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Dapplo.Config.Test.TestInterfaces {
	public enum WindowCaptureMode {
		Screen,
		GDI,
		Aero,
		AeroTransparent,
		Auto
	}

	public enum BuildStates {
		UNSTABLE,
		RELEASE_CANDIDATE,
		RELEASE
	}

	public enum ClickActions {
		DO_NOTHING,
		OPEN_LAST_IN_EXPLORER,
		OPEN_LAST_IN_EDITOR,
		OPEN_SETTINGS,
		SHOW_CONTEXT_MENU
	}

	/// <summary>
	///     The capture mode for Greenshot
	/// </summary>
	public enum CaptureMode {
		None,
		Region,
		FullScreen,
		ActiveWindow,
		Window,
		LastRegion,
		Clipboard,
		File,
		IE,
		Import
	}; //, Video };

	public enum ScreenCaptureMode {
		Auto,
		FullScreen,
		Fixed
	};

	public enum OutputFormat {
		bmp,
		gif,
		jpg,
		png,
		tiff,
		greenshot
	}

	/// <summary>
	/// A container for the ImageOutputSettings
	/// </summary>
	public class ImageOutputSettings {
		public string Name {
			get;
			set;
		}
		public OutputFormat Format {
			get;
			set;
		}
		public int JPGQuality {
			get;
			set;
		}
		public bool AutoQuantize {
			get;
			set;
		}
		public bool ForceQuantize {
			get;
			set;
		}

		/// <summary>
		/// A list of effects that need to be applied, the string is a key to the effect information
		/// </summary>
		public List<string> Effects {
			get;
			set;
		}
	}

	/// <summary>
	/// Description of CoreConfiguration.
	/// </summary>
	[Description("Greenshot Core configuration")]
	[IniSection("Core")]
	public interface CoreConfiguration : IIniSection {
		#region General
		[Description("The language in IETF format (e.g. en-US)"), DefaultValue("en-US")]
		string Language {
			get;
			set;
		}

		[Description("Is this the first time launch?"), DefaultValue(true)]
		bool IsFirstLaunch {
			get;
			set;
		}

		[Description("The wav-file to play when a capture is taken, loaded only once at the Greenshot startup"), DefaultValue("default")]
		string NotificationSound {
			get;
			set;
		}
		#endregion

		#region Hotkeys
		[Description("Hotkey for starting the region capture"), DefaultValue("PrintScreen")]
		string RegionHotkey {
			get;
			set;
		}

		[Description("Hotkey for starting the window capture"), DefaultValue("Alt + PrintScreen")]
		string WindowHotkey {
			get;
			set;
		}

		[Description("Hotkey for starting the fullscreen capture"), DefaultValue("Ctrl + PrintScreen")]
		string FullscreenHotkey {
			get;
			set;
		}

		[Description("Hotkey for starting the last region capture"), DefaultValue("Shift + PrintScreen")]
		string LastregionHotkey {
			get;
			set;
		}

		[Description("Hotkey for starting the IE capture"), DefaultValue("Shift + Ctrl + PrintScreen")]
		string IEHotkey {
			get;
			set;
		}
		#endregion

		[Description("Which destinations? Possible options (more might be added by plugins) are: Editor, FileDefault, FileWithDialog, Clipboard, Printer, EMail, Picker"), DefaultValue("Picker"), TypeConverter(typeof(StringToGenericListConverter<string>))]
		List<string> OutputDestinations {
			get;
			set;
		}

		#region Clipboard
		[Description("Exports to clipboard contain PNG"), DefaultValue(true)]
		bool ClipboardWritePNG {
			get;
			set;
		}

		[Description("Exports to clipboard contain HTML"), DefaultValue(false)]
		bool ClipboardWriteHTML {
			get;
			set;
		}
		[Description("If HTML is exported to the clipboard, use a Data URL (Inline image)"), DefaultValue(false)]
		bool ClipboardWriteHTMLDataUrl {
			get;
			set;
		}
		[Description("Exports to clipboard contain DIB"), DefaultValue(true)]
		bool ClipboardWriteDIB {
			get;
			set;
		}

		[Description("Exports to clipboard contain a Bitmap"), DefaultValue(true)]
		bool ClipboardWriteBITMAP {
			get;
			set;
		}

		[Description("Enable a special DIB clipboard reader"), DefaultValue(true), Category("Expert")]
		bool EnableSpecialDIBClipboardReader {
			get;
			set;
		}
		#endregion

		[Description("Specify which destinations, and in what order, are shown in the destination picker. Empty (default) means all.")]
		List<string> PickerDestinations {
			get;
			set;
		}

		#region Capture
		[Description("Should the mouse be captured?"), DefaultValue(true)]
		bool CaptureMousepointer {
			get;
			set;
		}

		[Description("Use interactive window selection to capture? (false=Capture active window)"), DefaultValue(false)]
		bool CaptureWindowsInteractive {
			get;
			set;
		}

		[Description("Capture delay in millseconds."), DefaultValue(100), Category("Expert")]
		int CaptureDelay {
			get;
			set;
		}

		[Description("The capture mode used to capture a screen. (Auto, FullScreen, Fixed)"), DefaultValue(ScreenCaptureMode.Auto)]
		ScreenCaptureMode ScreenCaptureMode {
			get;
			set;
		}

		[Description("The screen number to capture when using ScreenCaptureMode Fixed."), DefaultValue(1)]
		int ScreenToCapture {
			get;
			set;
		}

		[Description("The capture mode used to capture a Window (Screen, GDI, Aero, AeroTransparent, Auto)."), DefaultValue(WindowCaptureMode.Auto)]
		WindowCaptureMode WindowCaptureMode {
			get;
			set;
		}

		[Description("Enable/disable capture all children, very slow but will make it possible to use this information in the editor."), DefaultValue(false), Category("Expert")]
		bool WindowCaptureAllChildLocations {
			get;
			set;
		}

		[Description("The background color for a DWM window capture.")]
		Color DWMBackgroundColor {
			get;
			set;
		}

		//[IniProperty("PlayCameraSound", LanguageKey = "settings_playsound", Description = "Play a camera sound after taking a capture.", DefaultValue = "false")]
		[Description("Play a camera sound after taking a capture."), DefaultValue(false)]
		bool PlayCameraSound {
			get;
			set;
		}

		[Description("Flash the screen after taking a capture."), DefaultValue(true)]
		bool ShowFlashlight {
			get;
			set;
		}

		[Description("Remove the corners from a window capture"), DefaultValue(true), Category("Expert")]
		bool WindowCaptureRemoveCorners {
			get;
			set;
		}

		[Description("Sets if the zoomer is enabled"), DefaultValue(true)]
		bool ZoomerEnabled {
			get;
			set;
		}

		[Description("The cutshape which is used to remove the window corners, is mirrorred for all corners)"), DefaultValue("5,3,2,1,1"), Category("Expert"), TypeConverter(typeof(StringToGenericListConverter<int>))]
		List<int> WindowCornerCutShape {
			get;
			set;
		}

		[Description("List of products for which GDI capturing is skipped (using fallback)."), DefaultValue("IntelliJ IDEA"), Category("Expert"), TypeConverter(typeof(StringToGenericListConverter<string>))]
		List<string> NoGDICaptureForProduct {
			get;
			set;
		}

		[Description("List of productnames for which DWM capturing is skipped (using fallback)."), DefaultValue("Citrix ICA Client"), Category("Expert"), TypeConverter(typeof(StringToGenericListConverter<string>))]
		List<string> NoDWMCaptureForProduct {
			get;
			set;
		}
		#endregion

		#region Output
		[Description("Output file path.")]
		string OutputFilePath {
			get;
			set;
		}

		[Description("If the target file already exists True will make Greenshot always overwrite and False will display a 'Save-As' dialog."), DefaultValue(true)]
		bool OutputFileAllowOverwrite {
			get;
			set;
		}

		[Description("Filename pattern for screenshot."), DefaultValue("${capturetime:d\"yyyy-MM-dd HH_mm_ss\"}-${title}")]
		string OutputFileFilenamePattern {
			get;
			set;
		}

		[Description("Default file type for writing screenshots. (bmp, gif, jpg, png, tiff)"), DefaultValue(OutputFormat.png)]
		OutputFormat OutputFileFormat {
			get;
			set;
		}

		[Description("If set to true, than the colors of the output file are reduced to 256 (8-bit) colors"), DefaultValue(false)]
		bool OutputFileReduceColors {
			get;
			set;
		}

		[Description("If set to true the amount of colors is counted and if smaller than 256 the color reduction is automatically used."), DefaultValue(false)]
		bool OutputFileAutoReduceColors {
			get;
			set;
		}

		[Description("When saving a screenshot, copy the path to the clipboard?"), DefaultValue(true)]
		bool OutputFileCopyPathToClipboard {
			get;
			set;
		}

		[Description("SaveAs Full path")]
		string OutputFileAsFullpath {
			get;
			set;
		}

		[Description("JPEG file save quality in %."), DefaultValue(80)]
		int OutputFileJpegQuality {
			get;
			set;
		}

		[Description("Ask for the quality before saving?"), DefaultValue(false)]
		bool OutputFilePromptQuality {
			get;
			set;
		}

		[Description("The number for the ${NUM} in the filename pattern, is increased automatically after each save."), DefaultValue(1)]
		uint OutputFileIncrementingNumber {
			get;
			set;
		}
		#endregion

		#region Print
		//[IniProperty("OutputPrintPromptOptions", LanguageKey = "settings_alwaysshowprintoptionsdialog", Description = "Ask for print options when printing?", DefaultValue = "true")]
		[Description("Ask for print options when printing?"), DefaultValue(true)]
		bool OutputPrintPromptOptions {
			get;
			set;
		}

		//[IniProperty("OutputPrintAllowRotate", LanguageKey = "printoptions_allowrotate", Description = "Allow rotating the picture for fitting on paper?", DefaultValue = "false")]
		[Description("Allow rotating the picture for fitting on paper?"), DefaultValue(false)]
		bool OutputPrintAllowRotate {
			get;
			set;
		}

		//[IniProperty("OutputPrintAllowEnlarge", LanguageKey = "printoptions_allowenlarge", Description = "Allow growing the picture for fitting on paper?", DefaultValue = "false")]
		[Description("Allow growing the picture for fitting on paper?"), DefaultValue(false)]
		bool OutputPrintAllowEnlarge {
			get;
			set;
		}

		//[IniProperty("OutputPrintAllowShrink", LanguageKey = "printoptions_allowshrink", Description = "Allow shrinking the picture for fitting on paper?", DefaultValue = "true")]
		[Description("Allow shrinking the picture for fitting on paper?"), DefaultValue(true)]
		bool OutputPrintAllowShrink {
			get;
			set;
		}

		//[IniProperty("OutputPrintCenter", LanguageKey = "printoptions_allowcenter", Description = "Center image when printing?", DefaultValue = "true")]
		[Description("Center image when printing?"), DefaultValue(true)]
		bool OutputPrintCenter {
			get;
			set;
		}

		//[IniProperty("OutputPrintInverted", LanguageKey = "printoptions_inverted", Description = "Print image inverted (use e.g. for console captures)", DefaultValue = "false")]
		[Description("Print image inverted (use e.g. for console captures)"), DefaultValue(false)]
		bool OutputPrintInverted {
			get;
			set;
		}

		//[IniProperty("OutputPrintGrayscale", LanguageKey = "printoptions_printgrayscale", Description = "Force grayscale printing", DefaultValue = "false")]
		[Description("Force grayscale printing"), DefaultValue(false)]
		bool OutputPrintGrayscale {
			get;
			set;
		}

		//[IniProperty("OutputPrintMonochrome", LanguageKey = "printoptions_printmonochrome", Description = "Force monorchrome printing", DefaultValue = "false")]
		[Description("Force monorchrome printing"), DefaultValue(false)]
		bool OutputPrintMonochrome {
			get;
			set;
		}

		[Description("Threshold for monochrome filter (0 - 255), lower value means less black"), DefaultValue(127)]
		byte OutputPrintMonochromeThreshold {
			get;
			set;
		}

		//[IniProperty("OutputPrintFooter", LanguageKey = "printoptions_timestamp", Description = "Print footer on print?", DefaultValue = "true")]
		[Description("Print footer on print?"), DefaultValue(true)]
		bool OutputPrintFooter {
			get;
			set;
		}

		[Description("Footer pattern"), DefaultValue("${capturetime:d\"D\"} ${capturetime:d\"T\"} - ${title}"), Category("Expert")]
		string OutputPrintFooterPattern {
			get;
			set;
		}
		#endregion

		#region IE
		[Description("Enable/disable IE capture"), DefaultValue(true)]
		bool IECapture {
			get;
			set;
		}

		[Description("Enable/disable IE field capture, very slow but will make it possible to annotate the fields of a capture in the editor."), DefaultValue(false)]
		bool IEFieldCapture {
			get;
			set;
		}

		[Description("Comma separated list of Window-Classes which need to be checked for a IE instance!"), DefaultValue("AfxFrameOrView70,IMWindowClass"), TypeConverter(typeof(StringToGenericListConverter<string>))]
		List<string> WindowClassesToCheckForIE {
			get;
			set;
		}
		#endregion

		#region Network & updates
		[Description("Use your global proxy?"), DefaultValue(true)]
		bool UseProxy {
			get;
			set;
		}

		[Description("How many days between every update check? (0=no checks)"), DefaultValue(1)]
		int UpdateCheckInterval {
			get;
			set;
		}

		[Description("Last update check"), DataMember(EmitDefaultValue = true)]
		DateTimeOffset LastUpdateCheck {
			get;
			set;
		}

		[Description("Also check for unstable version updates"), DefaultValue(false), Category("Expert")]
		bool CheckForUnstable {
			get;
			set;
		}
		#endregion

		#region Admin
		[Description("Enable/disable the access to the settings, can only be changed manually in this .ini"), DefaultValue(false)]
		bool DisableSettings {
			get;
			set;
		}

		[Description("Enable/disable the access to the quick settings, can only be changed manually in this .ini"), DefaultValue(false)]
		bool DisableQuickSettings {
			get;
			set;
		}

		[DataMember(Name = "DisableTrayicon"), Description("Disable the trayicon, can only be changed manually in this .ini"), DefaultValue(false)]
		bool HideTrayicon {
			get;
			set;
		}

		[Description("If set to true, show expert settings"), DefaultValue(false)]
		bool ShowExpertSettings {
			get;
			set;
		}

		[Description("Show expert checkbox in the settings, can only be changed manually in this .ini"), DefaultValue(true)]
		bool ShowExpertCheckbox {
			get;
			set;
		}

		[Description("Comma separated list of Plugins which are allowed. If something in the list, than every plugin not in the list will not be loaded!"), TypeConverter(typeof(StringToGenericListConverter<string>))]
		List<string> IncludePlugins {
			get;
			set;
		}

		[Description("Comma separated list of Plugins which are NOT allowed."), TypeConverter(typeof(StringToGenericListConverter<string>))]
		List<string> ExcludePlugins {
			get;
			set;
		}

		[Description("Comma separated list of destinations which should be disabled."), TypeConverter(typeof(StringToGenericListConverter<string>))]
		List<string> ExcludeDestinations {
			get;
			set;
		}
		#endregion

		#region Advanced

		[Description("Make some optimizations for usage with remote desktop 'like' applications"), DefaultValue(false), Category("Expert")]
		bool OptimizeForRDP {
			get;
			set;
		}

		[Description("Optimize memory footprint, but with a performance penalty!"), DefaultValue(false), Category("Expert")]
		bool MinimizeWorkingSetSize {
			get;
			set;
		}

		#endregion

		#region Title-fix processor
		[Description("The title-fixes that are active."), TypeConverter(typeof(StringToGenericListConverter<string>))]
		List<string> ActiveTitleFixes {
			get;
			set;
		}

		[Description("The regular expressions to match the title with.")]
		Dictionary<string, string> TitleFixMatcher {
			get;
			set;
		}

		[Description("The replacements for the matchers.")]
		Dictionary<string, string> TitleFixReplacer {
			get;
			set;
		}
		#endregion

		[DataMember(EmitDefaultValue = true), Description("A list of experimental features, this allows us to test certain features before releasing them."), TypeConverter(typeof(StringToGenericListConverter<string>))]
		List<string> ExperimentalFeatures {
			get;
			set;
		}

		#region TrayIcon
		//[IniProperty("ShowTrayNotification", LanguageKey = "settings_shownotify", Description = "Show a notification from the systray when a capture is taken.", DefaultValue = "true")]
		[Description("Show a notification from the systray when a capture is taken."), DefaultValue(true)]
		bool ShowTrayNotification {
			get;
			set;
		}

		[Description("Specify what action is made if the tray icon is left clicked, if a double-click action is specified this action is initiated after a delay (configurable via the windows double-click speed)"), DefaultValue(ClickActions.SHOW_CONTEXT_MENU)]
		ClickActions LeftClickAction {
			get;
			set;
		}

		[Description("Specify what action is made if the tray icon is double clicked"), DefaultValue(ClickActions.OPEN_LAST_IN_EXPLORER)]
		ClickActions DoubleClickAction {
			get;
			set;
		}

		[Description("Maximum length of submenu items in the context menu, making this longer might cause context menu issues on dual screen systems."), DefaultValue(25)]
		int MaxMenuItemLength {
			get;
			set;
		}


		[Description("Enable/disable thumbnail previews"), DefaultValue(true), Category("Expert")]
		bool ThumnailPreview {
			get;
			set;
		}
		#endregion

		#region MAPI
		[Description("The subject pattern for the email destination (settings for Outlook can be found under the Office section)"), DefaultValue("${title}")]
		string MailApiSubjectPattern {
			get;
			set;
		}

		[Description("The 'to' field for the email destination (settings for Outlook can be found under the Office section)")]
		string MailApiTo {
			get;
			set;
		}

		[Description("The 'CC' field for the email destination (settings for Outlook can be found under the Office section)")]
		string MailApiCC {
			get;
			set;
		}

		[Description("The 'BCC' field for the email destination (settings for Outlook can be found under the Office section)")]
		string MailApiBCC {
			get;
			set;
		}
		#endregion

		[Description("Optional command to execute on a temporary PNG file, the command should overwrite the file and Greenshot will read it back. Note: this command is also executed when uploading PNG's!"), Category("Expert")]
		string OptimizePNGCommand {
			get;
			set;
		}

		[Description("Arguments for the optional command to execute on a PNG, {0} is replaced by the temp-filename from Greenshot. Note: Temp-file is deleted afterwards by Greenshot."), DefaultValue("\"{0}\""), Category("Expert")]
		string OptimizePNGCommandArguments {
			get;
			set;
		}

		[Description("Version of Greenshot which created this .ini")]
		string LastSaveWithVersion {
			get;
			set;
		}

		[Description("When reading images from files or clipboard, use the EXIF information to correct the orientation"), DefaultValue(true), Category("Expert")]
		bool ProcessEXIFOrientation {
			get;
			set;
		}

		[IniPropertyBehavior(Write = false, Read = false), Description("Location of the last captured region")]
		System.Windows.Rect LastCapturedRegion {
			get;
			set;
		}

		[Description("Last used colors"), TypeConverter(typeof(StringToGenericListConverter<Color>))]
		List<Color> RecentColors {
			get;
			set;
		}

		[DataMember(EmitDefaultValue = true), DefaultValue(""), Description("Token."), TypeConverter(typeof(StringEncryptionTypeConverter))]
		string BoxToken {
			get;
			set;
		}
	}
}
