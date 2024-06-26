﻿/* Copyright (c) Cloud Software Group, Inc. 
 * 
 * Redistribution and use in source and binary forms, 
 * with or without modification, are permitted provided 
 * that the following conditions are met: 
 * 
 * *   Redistributions of source code must retain the above 
 *     copyright notice, this list of conditions and the 
 *     following disclaimer. 
 * *   Redistributions in binary form must reproduce the above 
 *     copyright notice, this list of conditions and the 
 *     following disclaimer in the documentation and/or other 
 *     materials provided with the distribution. 
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND 
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF 
 * SUCH DAMAGE.
 */

using XenAdmin.Core;
using XenAPI;


namespace XenAdmin.Actions
{
    public class UpdateIntegratedGpuPassthroughAction : AsyncAction
    {
        private bool enable;

        public UpdateIntegratedGpuPassthroughAction(Host host, bool enableOnNextReboot, bool suppressHistory)
            : base(host.Connection, string.Format(Messages.ACTION_UPDATE_INTEGRATED_GPU_PASSTHROUGH_TITLE, host.Name()), null, suppressHistory)
        {
            Host = host;
            enable = enableOnNextReboot;

            if (enable)
                ApiMethodsToRoleCheck.AddRange("Host.async_enable_display", "PGPU.async_enable_dom0_access");
            else
                ApiMethodsToRoleCheck.AddRange("Host.async_disable_display", "PGPU.async_disable_dom0_access");
        }

        protected override void Run()
        {
            Description = Messages.UPDATING_PROPERTIES;

            RelatedTask = enable 
                ? Host.async_enable_display(Session, Host.opaque_ref) 
                : Host.async_disable_display(Session, Host.opaque_ref);

            PollToCompletion(0, 50);

            var pGpu = Host.SystemDisplayDevice();
            if (pGpu != null)
            {
                RelatedTask = enable 
                    ? PGPU.async_enable_dom0_access(Session, pGpu.opaque_ref) 
                    : PGPU.async_disable_dom0_access(Session, pGpu.opaque_ref);
                PollToCompletion(50, 100);
            }

            Tick(100, string.Format(Messages.UPDATED_PROPERTIES, Helpers.GetName(Host).Ellipsise(50)));
        }
    }
}
