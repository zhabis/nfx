﻿/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2017 ITAdapter Corp. Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/
using System;

using NFX.ApplicationModel.Pile;
using NFX.Environment;
using NFX.Scripting;

namespace NFX.ITest.AppModel.Pile
{
    /// <summary>
    /// Base for all high-load tests
    /// </summary>
    public abstract class HighMemoryLoadTest : IRunnableHook
    {
        public abstract ulong MinRAM { get; }

        void IRunnableHook.Prologue(Runner runner, FID id)
        {
            System.GC.Collect();
            var ms = NFX.OS.Computer.GetMemoryStatus();

            var has = ms.TotalPhysicalBytes;
            if (has < MinRAM)
                Aver.Fail("The machine has to have at least {0:n0} bytes of ram for this test, but it only has {1:n0} bytes".Args(MinRAM, has));
        }

        bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error)
        {
           System.GC.Collect();
           return false;
        }
    }

    /// <summary>
    /// Base for all high-load tests for PCs with 32 GB RAM at least
    /// </summary>
    public class HighMemoryLoadTest32RAM : HighMemoryLoadTest
    {
        public override ulong MinRAM { get { return 32ul * 1000ul * 1000ul * 1000ul; } }
    }

    /// <summary>
    /// Base for all high-load tests for PCs with 64 GB RAM at least
    /// </summary>
    public class HighMemoryLoadTest64RAM : HighMemoryLoadTest
    {
        public override ulong MinRAM { get { return 64ul * 1000ul * 1000ul * 1000ul; } }
    }
}
