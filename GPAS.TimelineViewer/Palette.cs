using System;
using System.Windows.Media;

namespace GPAS.TimelineViewer
{
    internal static class Palette
    {
        static Random random = new Random();

        public static Brush GetColorFromIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return BaraRed;
                case 1:
                    return AndroidGreen;
                case 2:
                    return MerchantMarineBlue;
                case 3:
                    return Sunflower;
                case 4:
                    return ForgottenPurple;
                case 5:
                    return RedPigment;
                case 6:
                    return Energos;
                case 7:
                    return LavenderTea;
                case 8:
                    return TurkishAqua;
                case 9:
                    return VeryBerry;
                case 10:
                    return PixelatedGrass;
                case 11:
                    return RadiantYellow;
                case 12:
                    return MagentaPurple;
                case 13:
                    return CircumorbitalRing;
                case 14:
                    return LavenderRose;
                case 15:
                    return LeaguesUnderTheSea20000;
                case 16:
                    return MediterraneanSea;
                case 17:
                    return Hollyhock;
                case 18:
                    return BlueMartina;
                case 19:
                    return PuffinsBill;
                default:
                    return new SolidColorBrush(new Color()
                    {
                        A = 255,
                        R = (byte)random.Next(0, 255),
                        G = (byte)random.Next(0, 255),
                        B = (byte)random.Next(0, 255),
                    });
            }
        }

        public static Brush RedPigment { get; private set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EA2027"));
        public static Brush PixelatedGrass { get; private set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#009432"));
        public static Brush MerchantMarineBlue { get; private set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0652DD"));
        public static Brush MagentaPurple { get; private set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6F1E51"));
        public static Brush CircumorbitalRing { get; private set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5758BB"));

        public static Brush PuffinsBill { get; private set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EE5A24"));
        public static Brush TurkishAqua { get; private set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#006266"));
        public static Brush LeaguesUnderTheSea20000 { get; private set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1B1464"));
        public static Brush Hollyhock { get; private set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#833471"));
        public static Brush ForgottenPurple { get; private set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9980FA"));

        public static Brush RadiantYellow { get; private set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F79F1F"));
        public static Brush AndroidGreen { get; private set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A3CB38"));
        public static Brush VeryBerry { get; private set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B53471"));
        public static Brush MediterraneanSea { get; private set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1289A7"));
        public static Brush LavenderTea { get; private set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D980FA"));

        public static Brush Sunflower { get; private set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC312"));
        public static Brush Energos { get; private set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C4E538"));
        public static Brush BaraRed { get; private set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ED4C67"));
        public static Brush BlueMartina { get; private set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#12CBC4"));
        public static Brush LavenderRose { get; private set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FDA7DF"));
    }
}
