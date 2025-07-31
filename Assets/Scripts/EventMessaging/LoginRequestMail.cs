////////////////////////////////////////////////////////////////////////////////
// This file is auto-generated.
// Do not hand modify this file.
// It will be overwritten next time the generator is run.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BigBenchGames.Tools.MailmanDispatcher
{
    /// <summary>
    /// 
    /// </summary>
	[ReadOnly(false), Version(1), CachedHash(-283123565)]
    public class LoginRequestMail : Mail
    {
        ///<inheritdoc cref="Mail"/>
        public override int CachedHash => cachedHash;

        private static int cachedHash = -283123565;

        public LoginRequestMail()
        {

        }


        ///<inheritdoc cref="Mail"/>
        public override void Clear()
        {

        }

        ///<inheritdoc cref="Mail"/>
        public override string GetSourcePath()
        {
            return InternalGetSourcePath();
        }

        private string InternalGetSourcePath([CallerFilePath] string sourceFileName = default)
        {
            return sourceFileName;
        }
    }
}