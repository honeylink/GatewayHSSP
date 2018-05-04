using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LIBC
{

    class libc
    {
        // 标准仿C memcpy函数  
        unsafe public static void* memcpy(void* dst, void* src, uint count)
        {
            System.Diagnostics.Debug.Assert(dst != null);
            System.Diagnostics.Debug.Assert(src != null);

            void* ret = dst;

            /* 
            * copy from lower addresses to higher addresses 
            */
            while (count-- > 0)
            {
                *(char*)dst = *(char*)src;
                dst = (char*)dst + 1;
                src = (char*)src + 1;
            }

            return (ret);
        }

        // 标准仿C memmove函数  
        unsafe public static void* memmove(void* dst, void* src, uint count)
        {
            System.Diagnostics.Debug.Assert(dst != null);
            System.Diagnostics.Debug.Assert(src != null);

            void* ret = dst;

            if (dst <= src || (char*)dst >= ((char*)src + count))
            {
                while (count-- > 0)
                {
                    *(char*)dst = *(char*)src;
                    dst = (char*)dst + 1;
                    src = (char*)src + 1;
                }
            }
            else
            {
                dst = (char*)dst + count - 1;
                src = (char*)src + count - 1;
                while (count-- > 0)
                {
                    *(char*)dst = *(char*)src;
                    dst = (char*)dst - 1;
                    src = (char*)src - 1;
                }
            }

            return (ret);
        }

        // 标准仿C memset函数  
        unsafe public static void* memset(void* s, int c, uint n)
        {
            byte* p = (byte*)s;

            while (n > 0)
            {
                *p++ = (byte)c;
                --n;
            }

            return s;
        }

        // 标准仿C memset函数  
        unsafe public static int MIN(int a, int b)
        {
            return a > b ? b : a;
        }  
    }
}
