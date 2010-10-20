﻿/* SOURCE:  http://lazy.codeplex.com/
 * File:    http://lazy.codeplex.com/SourceControl/changeset/view/55373#307770
 * Author:  pablito900
 * Licence: GNU General Public License version 2 (GPLv2)
 */

#if DEBUG
using System;
using System.Collections.Generic;
using EnvDTE80;

namespace NzbDrone
{

    public class ProcessAttacher
    {
        private enum AttachType
        {
            Managed,
            Native,
            ManagedAndNative
        }

        private enum AttachResult
        {
            Attached,
            NotRunning,
            BeingDebugged
        }

        public static void Attach()
        {
            // Get an instance of the currently running Visual Studio IDE.
            DTE2 dte2;
            dte2 = (DTE2)System.Runtime.InteropServices.Marshal.
            GetActiveObject("VisualStudio.DTE.10.0");

            var pa = new ProcessAttacher(dte2, "iisexpress", 20);
            pa.OptimisticAttachManaged();
        }


        #region private

        private readonly Dictionary<AttachType, string> _attachTypesMap;
        private readonly DTE2 _dte;
        private readonly string _processName;
        private readonly int _waitTimeout;

        #endregion

        #region ctor

        private ProcessAttacher(DTE2 dte, string processName, int waitTimeout)
        {
            _processName = processName;
            _waitTimeout = waitTimeout;
            _dte = dte;
            _attachTypesMap = new Dictionary<AttachType, string> {
                                                                     {AttachType.Managed, "Managed"}
                                                                 };
        }

        #endregion

        #region private methods

        private AttachResult Attach(AttachType attachType)
        {
            string engine = _attachTypesMap[attachType];

            if (IsBeingDebugged())
            {
                return AttachResult.BeingDebugged;
            }

            var dbg = _dte.Debugger as Debugger2;
            var trans = dbg.Transports.Item("Default");

            var eng = trans.Engines.Item(engine);

            EnvDTE80.Process2 proc = null;

            try
            {
                proc = dbg.GetProcesses(trans, "").Item(_processName) as EnvDTE80.Process2;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Invalid index."))
                {
                    return AttachResult.NotRunning;
                }
            }

            proc.Attach2(eng);

            return AttachResult.Attached;

        }

        private AttachResult PessimisticAttach(AttachType attachType)
        {
            AttachResult res = Attach(attachType);

            DateTime timeout = DateTime.Now.AddSeconds(_waitTimeout);

            while (res == AttachResult.NotRunning && timeout > DateTime.Now)
            {
                res = Attach(attachType);
                System.Threading.Thread.Sleep(100);
            }
            return res;
        }

        private bool IsBeingDebugged()
        {
            if (_dte.Debugger.DebuggedProcesses != null)
            {
                foreach (EnvDTE.Process process in _dte.Debugger.DebuggedProcesses)
                {
                    if (process.Name.IndexOf(_processName) != -1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region public methods


        public void OptimisticAttachManaged()
        {
            Attach(AttachType.Managed);
        }

        public void PessimisticAttachManaged()
        {
            PessimisticAttach(AttachType.Managed);
        }

        #endregion
    }

#endif
}

