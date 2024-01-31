using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GPAS.PropertiesValidation
{
   public class GeoParser
    {

        private const string DegreePattern = @"
^\s*                 # Ignore any whitespace at the start of the string
(?<latSuf>[NS])?     # Optional suffix
(?<latDeg>.+?)       # Match anything and we'll try to parse it later
[D\*\u00B0]?\s*      # Degree symbol ([D|*|°] optional) followed by optional whitespace
(?<latSuf>[NS])?\s+  # Suffix could also be here. Need some whitespace to separate

(?<lonSuf>[EW])?     # Now try the longitude
(?<lonDeg>.+?)       # Degrees
[D\*\u00B0]?\s*      # Degree symbol + whitespace
(?<lonSuf>[EW])?     # Optional suffix
\s*$                 # Match the end of the string (ignoring whitespace)";

        private const string DegreeMinutePattern = @"
^\s*                 # Ignore any whitespace at the start of the string
(?<latSuf>[NS])?     # Optional suffix
(?<latDeg>.+?)       # Match anything
[D\*\u00B0\s]        # Degree symbol or whitespace
(?<latMin>.+?)       # Now look for minutes
[M'\u2032\u2019]?\s* # Minute symbol [single quote, prime, smart quote, M] + whitespace
(?<latSuf>[NS])?\s+  # Optional suffix + whitespace

(?<lonSuf>[EW])?      # Now try the longitude
(?<lonDeg>.+?)        # Degrees
[D\*\u00B0?\s]        # Degree symbol or whitespace
(?<lonMin>.+?)        # Minutes
[M'\u2032\u2019]?\s*  # Minute symbol
(?<lonSuf>[EW])?      # Optional suffix
\s*$                  # Match the end of the string (ignoring whitespace)";

        private const string DegreeMinuteSecondPattern = @"
^\s*                  # Ignore any whitespace at the start of the string
(?<latSuf>[NS])?      # Optional suffix
(?<latDeg>.+?)        # Match anything
[D\*\u00B0\s]         # Degree symbol/whitespace
(?<latMin>.+?)        # Now look for minutes
[M'\u2032\u2019\s]    # Minute symbol/whitespace
(?<latSec>.+?)        # Look for seconds
[""\u2033\u201D]?\s*  # Second symbol [double quote (c# escaped), double prime or smart doube quote] + whitespace
(?<latSuf>[NS])?\s+   # Optional suffix + whitespace

(?<lonSuf>[EW])?      # Now try the longitude
(?<lonDeg>.+?)        # Degrees
[D\*\u00B0\s]         # Degree symbol/whitespace
(?<lonMin>.+?)        # Minutes
[M'\u2032\u2019\s]    # Minute symbol/whitespace
(?<lonSec>.+?)        # Seconds
[""\u2033\u201D]?\s*  # Second symbol
(?<lonSuf>[EW])?      # Optional suffix
\s*$                  # Match the end of the string (ignoring whitespace)";

        private const string IsoPattern = @"
^\s*                                        # Match the start of the string, ignoring any whitespace
(?<latitude> [+-][0-9]{2,6}(?: \. [0-9]+)?) # The decimal digits and punctuation are strictly defined
(?<longitude>[+-][0-9]{3,7}(?: \. [0-9]+)?) # in the standard. The decimal part is optional.
(?<altitude> [+-][0-9]+(?: \. [0-9]+)?)?    # The altitude component is optional
/                                           # The string must be terminated by '/'";

        public const RegexOptions Options = RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase;

        public static readonly Regex degreeRegex =
            new Regex(DegreePattern, Options);
        public static readonly Regex degreeMinuteRegex =
            new Regex(DegreeMinutePattern, Options);
        public static readonly Regex degreeMinuteSecondRegex =
            new Regex(DegreeMinuteSecondPattern, Options);
        
        public static readonly Regex isoRegex =
            new Regex(IsoPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace);

    }
}
