using P5RPC.Fnaf2.Template.Configuration;
using Reloaded.Mod.Interfaces.Structs;
using System.ComponentModel;
using System.Reflection;

namespace P5RPC.Fnaf2.Configuration
{
	public class Config : Configurable<Config>
	{
		/*
      User Properties:
          - Please put all of your configurable properties here.

      By default, configuration saves as "Config.json" in mod user config folder.    
      Need more config files/classes? See Configuration.cs

      Available Attributes:
      - Category
      - DisplayName
      - Description
      - DefaultValue

      // Technically Supported but not Useful
      - Browsable
      - Localizable

      The `DefaultValue` attribute is used as part of the `Reset` button in Reloaded-Launcher.
    */

    [Category("Jumpscare Chance")]
    [DisplayName("Random Number Range: Begin")]
		[Description("The inclusive value of the random number range, i.e. it includes this value. This is also compared " +
      "against the random number to determine if a jumpscare will occur.\r\nFor example, via a range of 0 - 10,000," +
      " any random number generated from that range will be compared against zero.")]
		[DefaultValue(0)]
		public int Begin { get; set; } = 0;

    [Category("Jumpscare Chance")]
    [DisplayName("Random Number Range: End")]
		[Description("The exclusive value of the random number range, i.e. it excludes the value. If this is assigned as " +
      "10,000, it will not include 10,000.")]
		[DefaultValue(10000)]
		public int End { get; set; } = 10000;

    [Category("Timing")]
    [DisplayName("Interval: Milliseconds")]
    [Description("The interval at which jumpscares could potentially occur.")]
    [DefaultValue(1000)]
    public int Interval { get; set; } = 1000;

    [Category("Debugging")]
    [DisplayName("Randomization: Logging")]
    [Description("Always prints the randomly generated value to the console, regardless of whether a jumpscare occurs.")]
    [DefaultValue(false)]
    public bool RandomizationDebug { get; set; }
  }

  /// <summary>
  /// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
  /// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
  /// </summary>
	public class ConfiguratorMixin : ConfiguratorMixinBase
	{
		// 
	}
}