/*
 * Copyright © 2011 
 * Rowe Technology Inc.
 * All rights reserved.
 * http://www.rowetechinc.com
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification is NOT permitted.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
 * FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE 
 * COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
 * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 * 
 * HISTORY
 * -----------------------------------------------------------------
 * Date            Initials    Version    Comments
 * -----------------------------------------------------------------
 * 02/27/2012      RC          2.04       Initial coding
 * 09/12/2012      RC          2.15       Added a try/catch block in SetPixelRect().  When debugging and creating a break, the code would throw an exception.
 * 12/06/2012      RC          2.17       Added a try/catch block in SetPixelUnlocked().  Error occured when the ADCP hung.
 * 
 */

using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;
using System;
using System.Diagnostics;
using log4net;

/// <summary>
/// Code found at: http://msdn.microsoft.com/en-us/library/system.windows.media.imaging.writeablebitmap.aspx
/// Create a simpler interface to read and write pixels
/// to a writeable bitmap.
/// 
/// ALL METHOD CALLS TO THIS OBJECT MUST USE THE UI THREAD.
/// This includes the constructor.
/// If you do not use the UI thread for all the SimpleBitmap methods, you will get the 
/// error: "Must create DependencySource on same Thread as the DependencyObject".
/// 
/// Application.Current.Dispatcher.BeginInvoke(new Action(() => MethodCall()));
/// </summary>
class SimpleBitmap
{
    /// <summary>
    /// Setup logger to report errors.
    /// </summary>
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    /// <summary>
    /// WriteableBitmap that will store the 
    /// data.
    /// </summary>
    public WriteableBitmap BaseBitmap { get; private set; }

    /// <summary>
    /// Create a writeable bitmap based
    /// off the width and height given.
    /// </summary>
    /// <param name="width">Width of the bitmap.</param>
    /// <param name="height">Height of the bitmap.</param>
    public SimpleBitmap(int width, int height)
    {
        try
        {
            BaseBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
        }
        catch (Exception ex)
        {
            log.Debug("Error Creating Bitmap", ex);
        }
    }

    /// <summary>
    /// Get pixel color from the given x, y location
    /// in the bitmap.
    /// </summary>
    /// <param name="x">X location (Width).</param>
    /// <param name="y">Y Location (Height).</param>
    /// <returns>Color at the given X,Y location.</returns>
    public uint GetPixel(int x, int y)
    {
        unsafe
        {
            return *(uint*)(((byte*)BaseBitmap.BackBuffer.ToPointer()) + BaseBitmap.BackBufferStride * y + x * 4);
        }
    }

    /// <summary>
    /// Set the given color to the X, Y location of 
    /// the bitmap image.  This will write the pixel based 
    /// off the instructions given in the documentation.
    /// 
    /// 1. Call the Lock method to reserve the back buffer for updates
    /// 2. Obtain a pointer to the back buffer by accessing the BackBuffer property.
    /// 3. Write changes to the back buffer. Other threads may write changes to the back buffer when the WriteableBitmap is locked.
    /// 4. Call the AddDirtyRect method to indicate areas that have changed.
    /// 5. Call the Unlock method to release the back buffer and allow presentation to the screen.
    /// </summary>
    /// <param name="x">X location (Width).</param>
    /// <param name="y">Y Location (Height).</param>
    /// <param name="c">Color to set.</param>
    public void SetPixel(int x, int y, uint c)
    {
        BaseBitmap.Lock();
        SetPixelUnlocked(x, y, c);
        BaseBitmap.AddDirtyRect(new Int32Rect(x, y, 1, 1));
        BaseBitmap.Unlock();
    }

    /// <summary>
    /// This will write a block of pixels based off the width and height given.
    /// This will start at the x location, and go out (width lenght).  It will go down
    /// from Y to (Y + height).  It will fill in all the pixel between x and (x+width)
    /// and  Y and (Y + height).
    /// 
    /// This will follow the same rules above, lock, write, set dirty, unlock.
    /// </summary>
    /// <param name="x">X location start.</param>
    /// <param name="y">Y location start.</param>
    /// <param name="width">Width size in pixels.</param>
    /// <param name="height">Height size in pixels.</param>
    /// <param name="colorBrush">Color to write.</param>
    public void SetPixelRect(int x, int y, int width, int height, SolidColorBrush colorBrush)
    {
        int rectWidth = x + width;
        int rectHeight = y + height;
        uint c = ComputeColor(colorBrush);

        if (BaseBitmap != null)
        {

            BaseBitmap.Lock();
            try
            {
                for (int rectX = x; rectX < rectWidth; rectX++)
                {
                    for (int rectY = y; rectY < rectHeight; rectY++)
                    {
                        SetPixelUnlocked(rectX, rectY, c);
                        BaseBitmap.AddDirtyRect(new Int32Rect(rectX, rectY, 1, 1));
                    }
                }
            }
            catch (ArgumentException ex)
            {
                log.Debug("Error SimpleBitmap::SetPixelRect", ex);
            }
            BaseBitmap.Unlock();
        }
    }

    /// <summary>
    /// Write the color to the given X,Y pixel
    /// location.  This will all unsafe code.
    /// </summary>
    /// <param name="x">X location (Width).</param>
    /// <param name="y">Y location (Height).</param>
    /// <param name="c">Color to set.</param>
    private void SetPixelUnlocked(int x, int y, uint c)
    {
        try
        {
            // Ensure the x and y are not outside the range
            if (x < BaseBitmap.Width && y < BaseBitmap.Height)
            {
                unsafe
                {
                    *(uint*)(((byte*)BaseBitmap.BackBuffer.ToPointer()) + BaseBitmap.BackBufferStride * y + x * 4) = c;
                }
            }
            else
            {
                Debug.WriteLine(string.Format("SimpleBitmap::SetPixelUnlocked() Bad Value {0} {1} {2} {3}", x, BaseBitmap.Width, y, BaseBitmap.Height));
            }
        }
        catch (Exception e)
        {
            log.Error("Error unlocking a pixel.", e);
        }
    }

    /// <summary>
    /// Get the color from the given pixel location.
    /// This will call unsafe code.
    /// </summary>
    /// <param name="x">X location (Width).</param>
    /// <param name="y">Y location (Height).</param>
    /// <returns>Color at given X,Y location.</returns>
    public Color GetColor(int x, int y)
    {
        var p = BitConverter.GetBytes(GetPixel(x, y));
        return Color.FromArgb(p[3], p[2], p[1], p[0]);
    }

    /// <summary>
    /// Set the given color at the X,Y location
    /// given.  The user has to option to write
    /// to the pixel in a locked or unlocked manner.
    /// Unlocked will give a performance improvement
    /// but may cause issues writing to data being
    /// used by another thread.
    /// </summary>
    /// <param name="x">X Location (Width).</param>
    /// <param name="y">Y Location (Height).</param>
    /// <param name="c">Color.</param>
    /// <param name="perfromLock">TRUE = Use a lock to write the to the pixel.</param>
    public void SetColor(int x, int y, Color c, bool perfromLock)
    {
        if (perfromLock)
            SetPixel(x, y, (uint)c.A << 24 | ((uint)c.R << 16) | ((uint)c.G << 8) | c.B);
        else
            SetPixelUnlocked(x, y, (uint)c.A << 24 | ((uint)c.R << 16) | ((uint)c.G << 8) | c.B);
    }

    /// <summary>
    /// Convert the color brush to a uint representing
    /// the color.  The color is computed using
    /// the RGB value.
    /// </summary>
    /// <param name="colorBrush">Color brush to convert.</param>
    /// <returns>uint of the color stored in the color brush.</returns>
    public uint ComputeColor(SolidColorBrush colorBrush)
    {
        // Compute the pixel's color.
        uint color_data = (uint)colorBrush.Color.R << 16; // R
        color_data |= (uint)colorBrush.Color.G << 8;   // G
        color_data |= (uint)colorBrush.Color.B << 0;   // B

        return color_data;
    }
}