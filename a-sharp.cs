// A.ASM replacement using C# ported by Justin Marshall from the JonoF port
// Mainly by Ken Silverman, with things melded with my port by
// Jonathon Fowler (jonof@edgenetwork.org)

// "Build Engine & Tools" Copyright (c) 1993-1997 Ken Silverman
// Ken Silverman's official web site: "http://www.advsys.net/ken"
// See the included license file "BUILDLIC.TXT" for license info.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DukeMapT3D
{
    public static class A
    {
        public const int BITSOFPRECISION = 3;
        public const int BITSOFPRECISIONPOW = 8;

        public static int asm1, asm2, asm4, fpuasm, globalx3, globaly3;
        public static int asm3, asm5;

        public static int bpl, transmode = 0;
        static int glogx, glogy, gbxinc, gbyinc, gpinc;
        static byte[] gtrans;
        static byte[] gbuf;
        static int gbufpos, gpalpos, ghlinepalpos, gtranspos;

        //Global variable functions
        public static void setvlinebpl(int dabpl) { bpl = dabpl; }
        public static void fixtransluscence(byte[] datransoff, int pos)
        {
            gtrans = datransoff;
            gtranspos = pos;
        }
        public static void settransnormal() { transmode = 0; }
        public static void settransreverse() { transmode = 1; }

        public static bool forceRenderBlack = false;


        //Ceiling/floor horizontal line functions
        public static void sethlinesizes(int logx, int logy, byte[] bufplc, int bufpos)
        {
            glogx = logx;
            glogy = logy;
            gbuf = bufplc;
            gbufpos = bufpos;
        }

        public static void setpalookupaddress(int palookupnum, int pos)
        {
            throw new Exception("Not implemented");
        }

        public static void setuphlineasm4(int bxinc, int byinc)
        {
            gbxinc = bxinc;
            gbyinc = byinc;
        }

        public static void hlineasm4(int cnt, int skiploadincs, int paloffs, uint by, uint bx, int pbase)
        {
#if false
            //   if (skiploadincs == 0) 
            //    { 
            gbxinc = asm1; 
                gbyinc = asm2; 
         //   }
        
	        for(;cnt>=0;cnt--)
	        {
                if (forceRenderBlack)
                {
                    Engine._device._screenbuffer.Pixels[pbase] = 0;
                }
                else
                {
                    Engine._device._screenbuffer.Pixels[pbase] = Engine._device._palette._palettebuffer[Engine.palette._palookup[Engine.palette.globalpalwritten].palookup[(ghlinepalpos + paloffs) + gbuf[gbufpos + ((bx >> (32 - glogx) << glogy) + (by >> (32 - glogy)))]]];
                }
		        bx -=(uint)gbxinc;
                by -= (uint)gbyinc;
                pbase--;
	        }
#endif
        }



        //Sloped ceiling/floor vertical line functions
        public static void setupslopevlin(int logylogx, byte[] bufplc, int bufplcpos, int pinc)
        {
            glogx = (logylogx & 255); glogy = (logylogx >> 8);
            gbuf = bufplc; gpinc = pinc;
            gbufpos = bufplcpos;
        }
        public static void slopevlin(int startpos, int i, int[] slopalptr2, int slopalbase, int cnt, int bx, int by)
        {
#if false
            int bz;
            int bzinc;
	        uint u, v;

            bz = asm5; bzinc = (asm1 >> 3);

            byte[] pal = Engine.palette._palookup[Engine.globalpal].palookup;

            int ppos = startpos;
	        for(;cnt>0;cnt--)
	        {
		        i = pragmas.krecipasm(bz>>6); bz += bzinc;
                u = (uint)(bx + globalx3 * i);
                v = (uint)(by + globaly3 * i);

                if (forceRenderBlack)
                {
                    Engine._device._screenbuffer.Pixels[ppos] = 0;
                }
                else
                    Engine._device._screenbuffer.Pixels[ppos] = Engine._device._palette._palettebuffer[pal[gbuf[gbufpos + ((u >> (32 - glogx)) << glogy) + (v >> (32 - glogy))] + slopalptr2[slopalbase]]];
                slopalbase--;
		        //p += gpinc;
                ppos += gpinc;
	        }
#endif
        }


        //Wall,face sprite/wall sprite vertical line functions
        public static void setupvlineasm(int neglogy) { glogy = neglogy; }
        public static void vlineasm1(int vinc, int paloffs, int cnt, uint vplc, byte[] gbuf2, int basebuf, int pbase)
        {
#if false
            gbuf = gbuf2;
            gbufpos = basebuf;   
            gpalpos = paloffs;

	        for(;cnt>=0;cnt--)
	        {
                long pos = gbufpos + (vplc >> glogy);
                if (pos < 0)
                    continue;

                if (forceRenderBlack)
                {
                    Engine._device._screenbuffer.Pixels[pbase] = 0;
                }
                else
                    Engine._device._screenbuffer.Pixels[pbase] = Engine._device._palette._palettebuffer[Engine.palette._palookup[Engine.globalpal].palookup[gpalpos + gbuf[pos]]];
                pbase += bpl;
                vplc += (uint)vinc;
	        }
#endif
        }

        public static void setupmvlineasm(int neglogy) { glogy = neglogy; }
        public static void mvlineasm1(int vinc, byte[] paloffs, int paloffspos, int cnt, uint vplc, byte[] bufplc, int bufplcpos, int bufplcbase, int p, int tsizx, int tsizy)
        {
#if false
            byte ch;

	        gbuf = bufplc;
            gbufpos = bufplcpos;
	        //gpal = paloffs;
            gpalpos = paloffspos;

	        for(;cnt>=0;cnt--)
	        {
                long pos = bufplcbase + (vplc >> (glogy));
                if (pos >= gbuf.Length || pos < 0)
                    continue; 
                ch = gbuf[pos];
                if (ch != 255)
                {
                    if (forceRenderBlack)
                    {
                        Engine._device._screenbuffer.Pixels[p] = 0;
                    }
                    else
                        Engine._device._screenbuffer.Pixels[p] = Engine._device._palette._palettebuffer[Engine.palette._palookup[Engine.globalpal].palookup[gpalpos + ch]];
                }
		        p += bpl;
                vplc += (uint)vinc;
	        }
#endif
        }

        public static void setuptvlineasm(int neglogy) { glogy = neglogy; }
        public static void tvlineasm1(int vinc, byte[] paloffs, int paloffspos, int cnt, uint vplc, byte[] bufplc, int bufplcbase, int p)
        {
#if false
            byte ch;

	        gbuf = bufplc;
            gbufpos = bufplcbase;
	        //gpal = paloffs;
            gpalpos = paloffspos;

	        if (transmode != 0)
	        {
		        for(;cnt>=0;cnt--)
		        {
                    ch = gbuf[bufplcbase + (vplc >> glogy)];
                    if (ch != 255)
                    {
                        if (forceRenderBlack)
                        {
                            Engine._device._screenbuffer.Pixels[p] = 0;
                        }
                        else
                            Engine._device._screenbuffer.Pixels[p] = Engine._device._palette._palettebuffer[gtrans[(Engine.palette._palookup[Engine.globalpal].palookup[ch + gpalpos] << 8) + gtranspos]];
                    }
			        p += bpl;
                    vplc += (uint)vinc;
		        }
	        }
	        else
	        {
		        for(;cnt>=0;cnt--)
		        {
			        ch = gbuf[bufplcbase + (vplc>>glogy)];
                    if (ch != 255)
                    {
                        //Engine._device.SetScreenPixel(p, gtrans[(p/*p.memory*/<< 8) + Engine.palette._palookup[Engine.globalpal].palookup[ch + gpalpos] + gtranspos]);
                        int c = p/*.memory*/+ (Engine.palette._palookup[Engine.globalpal].palookup[ch + gpalpos] << 8) + gtranspos; // jmarshall: out of bounds check.
                        if (c < gtrans.Length)
                        {
                            int d = gtrans[c];
                            if (d < Engine._device._palette._palettebuffer.Length)
                                if (forceRenderBlack)
                                {
                                    Engine._device._screenbuffer.Pixels[p] = 0;
                                }
                                else
                                    Engine._device._screenbuffer.Pixels[p] = Engine._device._palette._palettebuffer[d];
                        }
                    }
			        p += bpl;
                    vplc += (uint)vinc;
		        }
	        }
#endif
        }

        //Floor sprite horizontal line functions
        public static void msethlineshift(int logx, int logy) { glogx = logx; glogy = logy; }
        public static void mhline(byte[] bufplc, int bufplcpos, uint bx, int cntup16, int junk, uint by, int p)
        {
#if false
            byte ch;

	        gbuf = bufplc;
            gbufpos = bufplcpos;
	        //gpal = Engine.palette._palookup[Engine.globalpal].palookup;
            gpalpos = asm3;

	        for(cntup16>>=16;cntup16>0;cntup16--)
	        {
                ch = gbuf[gbufpos + ((bx>>(32-glogx))<<glogy)+(by>>(32-glogy))];
                if (ch != 255)
                {
                    //   Engine._device.SetScreenPixel(p, Engine.palette._palookup[Engine.globalpal].palookup[ch + gpalpos]);
                    if (forceRenderBlack)
                    {
                        Engine._device._screenbuffer.Pixels[p] = 0;
                    }
                    else
                        Engine._device._screenbuffer.Pixels[p] = Engine._device._palette._palettebuffer[Engine.palette._palookup[Engine.globalpal].palookup[ch + gpalpos]];
                }
                bx += (uint)asm1;
                by += (uint)asm2;
		        p++;
	        }
#endif
        }

        public static void tsethlineshift(int logx, int logy)
        {
            glogx = logx;
            glogy = logy;
        }

        public static void thline(byte[] bufplc, int bufplcbase, uint bx, int cntup16, int junk, uint by, int p)
        {
#if false
            byte ch;

	        gbuf = bufplc;
            gbufpos = bufplcbase;
            //gpal = Engine.palette._palookup[Engine.globalpal].palookup;
            gpalpos = asm3;
	        if (transmode != 0)
	        {
		        for(cntup16>>=16;cntup16>0;cntup16--)
		        {
                    ch = gbuf[gbufpos + ((bx >> (32 - glogx)) << glogy) + (by >> (32 - glogy))];
                    if (ch != 255) // jv?
                    {
                        int _pallookup = Engine.palette._palookup[Engine.globalpal].palookup[ch + gpalpos] << 8;
                        byte trans = gtrans[(_pallookup) + gtranspos];

                        if (forceRenderBlack)
                        {
                            Engine._device._screenbuffer.Pixels[p] = 0;
                        }
                        else
                            Engine._device._screenbuffer.Pixels[p] = Engine._device._palette._palettebuffer[trans];
                    }
                    bx += (uint)asm1;
                    by += (uint)asm2;
			        p++;
		        }
	        }
	        else
	        {
		        for(cntup16>>=16;cntup16>0;cntup16--)
		        {
                    ch = gbuf[gbufpos + ((bx >> (32 - glogx)) << glogy) + (by >> (32 - glogy))];
			        if (ch != 255) /// jv
                    {
                        //Engine._device.SetScreenPixel(p, gtrans[((p/*.memory*/) << 8) + Engine.palette._palookup[Engine.globalpal].palookup[ch + gpalpos] + gtranspos]);

                        if (forceRenderBlack)
                        {
                            Engine._device._screenbuffer.Pixels[p] = 0;
                        }
                        else
                            Engine._device._screenbuffer.Pixels[p] = Engine._device._palette._palettebuffer[gtrans[((p/*.memory*/) << 8) + Engine.palette._palookup[Engine.globalpal].palookup[ch + gpalpos] + gtranspos]];
                    }
                    bx += (uint)asm1;
                    by += (uint)asm2;
			        p++;
		        }
	        }
#endif
        }


        //Rotatesprite vertical line functions
        public static void setupspritevline(byte[] paloffs, int palpos, int bxinc, int byinc, int ysiz)
        {
            //gpal = paloffs;
            gpalpos = palpos;
            gbxinc = bxinc;
            gbyinc = byinc;
            glogy = ysiz;
        }
        public static void spritevline(int bx, int by, int cnt, byte[] bufplc, int bufplcbase, int p)
        {
            throw new Exception("Not implemented");
        }

        //Rotatesprite vertical line functions
        public static void msetupspritevline(byte[] paloffs, int paloffspos, int bxinc, int byinc, int ysiz)
        {
            //gpal = paloffs;
            gpalpos = paloffspos;
            gbxinc = bxinc;
            gbyinc = byinc;
            glogy = ysiz;
        }
        public static void mspritevline(int bx, int by, int cnt, byte[] bufplc, int bufplcbase, int p)
        {

            throw new Exception("Not implemented");

        }

        public static void DrawPixelPallete(int pos, byte ch)
        {
            throw new Exception("Not implemented");
        }

        public static void tsetupspritevline(byte[] paloffs, int paloffspos, int bxinc, int byinc, int ysiz)
        {
            /*
	      //  gpal = paloffs;
            gpalpos = paloffspos;
	        gbxinc = bxinc;
	        gbyinc = byinc;
	        glogy = ysiz;
            */
        }
        public static void tspritevline(int bx, int by, int cnt, byte[] bufplc, int bufplcbase, int p)
        {
            byte ch;
#if false
            gbuf = bufplc;
            gbufpos = bufplcbase;
	        if (transmode != 0)
	        {
		        for(;cnt>1;cnt--)
		        {
                    ch = gbuf[gbufpos + ((bx >> 16) * glogy + (by >> 16))];
                    if (ch != 255)
                    {
                        //Engine._device.SetScreenPixel(p, gtrans[(p/*.memory*/) + (Engine.palette._palookup[Engine.globalpal].palookup[ch + gpalpos] << 8) + gtranspos]);

                        if (forceRenderBlack)
                        {
                            Engine._device._screenbuffer.Pixels[p] = 0;
                        }
                        else
                            Engine._device._screenbuffer.Pixels[p] = Engine._device._palette._palettebuffer[gtrans[(p/*.memory*/) + (Engine.palette._palookup[Engine.globalpal].palookup[ch + gpalpos] << 8) + gtranspos]];
                    }
			        bx += gbxinc;
			        by += gbyinc;
			        p += bpl;
		        }
	        }
	        else
	        {
		        for(;cnt>1;cnt--)
		        {
                    ch = gbuf[gbufpos + ((bx >> 16) * glogy + (by >> 16))];
                    if (ch != 255)
                    {
                        //   Engine._device.SetScreenPixel(p, gtrans[((p/*.memory*/) << 8) + Engine.palette._palookup[Engine.globalpal].palookup[ch + gpalpos] + gtranspos]);

                        if (forceRenderBlack)
                        {
                            Engine._device._screenbuffer.Pixels[p] = 0;
                        }
                        else
                            Engine._device._screenbuffer.Pixels[p] = Engine._device._palette._palettebuffer[gtrans[((p/*.memory*/) << 8) + Engine.palette._palookup[Engine.globalpal].palookup[ch + gpalpos] + gtranspos]];
                    }
			        bx += gbxinc;
			        by += gbyinc;
			        p += bpl;
		        }
	        }
#endif
        }

        public static void setupdrawslab(int dabpl, int pal, int offset)
        {
            throw new Exception("Not implemented");
        }

        public static void drawslab(int dx, int v, int dy, int vi, byte[] vptr, int vptrpos, int p)
        {
            throw new Exception("Not implemented");
        }
    }
}
