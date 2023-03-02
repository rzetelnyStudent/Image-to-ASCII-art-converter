using System.Drawing;

namespace CsConverter
{
    public static class CsConverter
    {
        private static char[] asciiChars = { '#', '#', '@', '%', '=', '+', '*', ':', '-', '.', ' ' };

        unsafe static void convertLineCs(int lineNr, byte* imageInBytesPtr, char* textLinePtr, int asciiImWidth)
        {

            int start_pos = lineNr * asciiImWidth * 3;     // calculate starting position in image RGBRGB... array
            int start_pos_ascii = lineNr * asciiImWidth;      // calculate starting position in output chars array      
            for (int w = 0; w < asciiImWidth * 3; w += 3)
            {
                // load pixel from RGBRGBRGB... array
                Color pixelColor = Color.FromArgb(*(imageInBytesPtr + start_pos + w), *(imageInBytesPtr + start_pos + w + 1), *(imageInBytesPtr + start_pos + w + 2));

                //Average out the RGB components to find the Gray Color
                int avg = ((int)pixelColor.R + (int)pixelColor.G + (int)pixelColor.B) / 3;

                int index = (avg * 10) / 255;       // convert to array with ascii chars index

                *(textLinePtr + start_pos_ascii + (w / 3)) = asciiChars[index];     // save char to output array
            }
        }
    }
}
