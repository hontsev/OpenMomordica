/*
 *	�˴����� T4 ������� StatusExport.tt ģ������, �������˽����´�����ô�, �����޸�!
 *	
 *	���ļ�������Ŀ Json �ļ�����������������.
 */
using System;
using System.Runtime.InteropServices;
using System.Text;
using Native.Csharp.App.EventArgs;
using Native.Csharp.App.Interface;
using Unity;

namespace Native.Csharp.App.Core
{
    public class StatusExport
    {
		#region --���캯��--
		/// <summary>
		/// ��̬���캯��, ע������ע��ص�
		/// </summary>
        static StatusExport ()
        {
			// �ַ�Ӧ�����¼�
			ResolveAppbackcall ();
        }
        #endregion

		#region --˽�з���--
		/// <summary>
		/// ��ȡ���е�ע����, �ַ�����Ӧ���¼�
		/// </summary>
		private static void ResolveAppbackcall ()
		{
			/*
			 * Name: CPUʹ����
			 * Function: _statusCPU
			 */
			if (Common.UnityContainer.IsRegistered<ICqStatus> ("CPUʹ����") == true)
			{
				Status_CPU = Common.UnityContainer.Resolve<ICqStatus> ("CPUʹ����").CqStatus;
			}


		}
        #endregion

		#region --��������--
		/*
		 * Id: 1
		 * Name: CPUʹ����
		 * Title: CPU
		 * Function: _statusCPU
		 * Period: 1000
		 */
		public static event EventHandler<CqStatusEventArgs> Status_CPU;
		[DllExport (ExportName = "_statusCPU", CallingConvention = CallingConvention.StdCall)]
		private static string Evnet__statusCPU ()
		{
			CqStatusEventArgs args = new CqStatusEventArgs (1, "CPUʹ����", "CPU", 1000);
			if (Status_CPU != null)
			{
				Status_CPU (null, args);
			}
			return args.FloatWindowData;
		}


        #endregion
	}
}

