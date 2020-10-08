using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Serilog;

namespace VulcanClient2
{
    public class Win32
    { 
        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref Point lpPoint);
    }

    public class CursorPos
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
    
    public class Mouse
    {
        private static CursorPos GetMouseCursorPos()
        {
            Point p = new Point();
            Win32.GetCursorPos(ref p);
            int xPos = p.X;
            int yPos = p.Y;
            
            Log.Debug($"{xPos} | {yPos}");
            return new CursorPos {X = xPos, Y = yPos};

        }
        public static void Move(int xP, int yP)
        {
            CursorPos curPos = GetMouseCursorPos();
            Point p = new Point();
            p.X = curPos.X + xP;
            p.Y = curPos.Y + yP;
            Win32.SetCursorPos(p.X, p.Y);
        }
        
        public static void MoveX(int xP)
        {
            CursorPos curPos = GetMouseCursorPos();
            Point p = new Point();
            p.X = curPos.X + xP;
            Win32.SetCursorPos(p.X, curPos.Y);
        }
        
        public static void MoveY(int yP)
        {
            CursorPos curPos = GetMouseCursorPos();
            Point p = new Point();
            p.Y = curPos.Y + yP;
            Win32.SetCursorPos(curPos.X, p.Y);
        }
    }
}