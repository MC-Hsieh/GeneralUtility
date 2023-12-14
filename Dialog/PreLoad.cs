using System;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;
using System.Drawing;
using System.Threading; 

namespace GeneralUtility.Dialog
{
	/// <summary>
	/// <para>本介面是以個預載視窗基本上是屬於流程安排的一種方式。</para>
	/// <para>流程撰寫方式如底下的範例程式。</para>
	/// </summary>
	public partial class XPreLoadViewer : Form
	{

		#region " Definition "

		/// <summary>最大預載進度數。</summary>
		private const int MAX_PRESTEP = 10;

		#endregion

		#region " Properties "

		/// <summary>當前進度的狀態。</summary>
		private int g_iStep;
		private SolidBrush g_clsBrush;
		private int g_iBarWidth;
		private int g_iBarHeight;
		/// <summary>動畫計數器，用來產生動畫用沒有任何意義。</summary>
		private int g_iAnimationCount;
		/// <summary>預載前進的計數。</summary>
		private int g_iPreStepCount;

		#endregion

		#region " Methods - New "

		/// <summary> 建立預載視窗 </summary>
		public XPreLoadViewer()
		{
			InitializeComponent();

			g_iStep = 0;
			g_clsBrush = new SolidBrush(Color.FromArgb(0, Color.White));
			g_iAnimationCount = 0;
			g_iPreStepCount = 0;
			g_iBarWidth = picNowStep.Width;
			g_iBarHeight = picNowStep.Height;
		}

		#endregion

		#region " Delegate "

		private delegate void UpdateString(string strInfo);

		#endregion

		#region " Methods "

		/// <summary>  當前進度狀況與說明 </summary>
		/// <param name="iNowStep"> 當前進度，範圍:0~100 </param>
		/// <param name="strCaption"> 當前進度描述 </param>
		public void NowStatus(int iNowStep, string strCaption)
		{
			g_iStep = iNowStep;
			g_iPreStepCount = 0;
			this.BeginInvoke(new MethodInvoker(delegate { picNowStep.Refresh(); }));
			this.BeginInvoke(new MethodInvoker(delegate { labStatus.Text = strCaption; }));
			Thread.Sleep(100);
		}

		/// <summary>設定標題。</summary>
		/// <param name="strTittle">標題名稱。 </param>
		public void SetTittle(string strTittle)
		{
			// 如果沒有設定標題的話則使用產品名稱
			if (strTittle == "")
			{
				strTittle = Application.ProductName;
			}
			this.Text = strTittle + "(Ver." + Application.ProductVersion + ")";
			labTittle.Text = strTittle;
			labVersion.Text = "Ver." + Application.ProductVersion;
		}

		/// <summary>設置當前進度。</summary>
		/// <param name="iStep">進度狀態 (0~100)之間的數值。</param>
		public void SetStep(int iStep)
		{
			g_iStep = iStep;
			g_iPreStepCount = 0;
			this.BeginInvoke(new MethodInvoker(delegate { picNowStep.Refresh(); }));
		}

		#endregion

		#region " Events "

		private void tmrAnimation_Tick(object sender, EventArgs e)
		{
			g_clsBrush.Color = Color.FromArgb((g_iAnimationCount * 10) % 255, Color.White);

			switch (g_iAnimationCount % 10)
			{
				case 0:
				case 9:
					g_clsBrush.Color = Color.FromArgb(0, Color.White);
					break;
				case 1:
				case 8:
					g_clsBrush.Color = Color.FromArgb(20, Color.White);
					break;
				case 2:
				case 7:
					g_clsBrush.Color = Color.FromArgb(40, Color.White);
					break;
				case 3:
				case 6:
					g_clsBrush.Color = Color.FromArgb(60, Color.White);
					break;
				case 4:
				case 5:
					g_clsBrush.Color = Color.FromArgb(80, Color.White);
					break;
				default:
					break;
			}

			g_iAnimationCount++;
			picNowStep.Refresh();

			if (g_iAnimationCount == 10)
			{
				if (g_iPreStepCount < 10)
				{
					g_iStep++;
				}
				g_iAnimationCount = 0;
				g_iPreStepCount++;
			}
		}

		private void XPreLoadViewer_Shown(object sender, EventArgs e)
		{
			this.TopMost = false; ;
			//tmrAnimation.Enabled = true;
		}

		#endregion

	}

	/// <summary>
	/// 用來建立程式一啟動的預載畫面控制。
	/// </summary>
	/// <example>
	/// <para>這是一個比較像是流程控制的方法，直接寫在自己的 <b>MainForm.cs</b> 內就可以了。</para>
	/// <code language="csharp" title="方法一(推薦)">
	/// class MainForm
	/// {
	/// 	// 建立預載流程控制器
	/// 	private static XPreLoadControl g_clsControl;
	/// 
	///		public MainForm()
	///		{
	///			//加入 preload 控制
	///			g_clsControl = new XPreLoadControl();
	///			// 如果底下這一行沒有給則會自動以產品名稱為預設
	///			g_clsControl.SetTittle("輸入你想要的標題");
	///			g_clsControl.AddAction(this.InitialSystemConfig,"初始設定檔");
	///			g_clsControl.AddAction(this.InitialCamera,"初始相機");
	///			g_clsControl.AddAction(this.InitialImageLibrary,"初始相機");
	///			g_clsControl.Start();
	/// 
	///			InitializeComponent();
	///		}
	///		
	///		private InitialSystemConfig()
	///		{  //初始內容 }
	///		
	///		private InitialCamera()
	///		{  //初始內容 }
	///		
	///		private InitialImageLibrary()
	///		{  //初始內容 }
	/// }
	/// </code>
	/// <code language="csharp" title="方法二(舊)">
	/// class MainForm
	/// {
	/// 	// 建立預載流程控制器
	/// 	private static XPreLoadControl g_clsControl;
	/// 
	///		public MainForm()
	///		{
	///			//加入 preload 控制
	///			g_clsControl = new XPreLoadControl(Initial);
	///			g_clsControl.Start();
	/// 
	///			InitializeComponent();
	///		}
	///		
	/// 	// 初始化所需要的動作項目
	/// 	// 並不是每一種初始動作都可以正常在這邊執行，例如跟UI相關的初始
	/// 	private void Initial()
	/// 	{
	/// 		// 設定標題或者你想要顯示的資訊，例如：版本號
	/// 		g_clsControl.SetTittle("DKen Ver " + FormMain.VERSION);
	/// 
	///			// 設定你要得初始動作的數量，以這邊的例子來說就是七個動作
	///			// 也就是會呼叫到 7 次的NowStatus()			
	/// 		g_clsControl.SetStepCount(7);
	/// 
	///			// 狀態描述，每呼叫一次 NowStatus() 進度會自動往前加一個級距
	/// 		g_clsControl.NowStatus("讀取設定檔");
	/// 		// 要初始的動作
	/// 		this.IniSystemConfig();
	/// 		
	/// 		g_clsControl.NowStatus("設定資料夾");
	/// 		this.IniDirectory();
	/// 
	/// 		g_clsControl.NowStatus("函式庫準備");
	/// 		this.IniDKenDll();
	/// 
	/// 		g_clsControl.NowStatus("設定網路連線");
	/// 		this.IniClient();
	/// 
	/// 		g_clsControl.NowStatus("初始攝影機");
	/// 		this.IniCamera();
	/// 
	/// 		g_clsControl.NowStatus("影像緩衝初始");
	/// 		this.IniImageBuffer();
	/// 
	///			// 記得最後加個結束字眼提醒使用者
	/// 		g_clsControl.NowStatus("完成");
	/// 
	/// 	}
	/// }
	/// </code>
	/// </example>
	public class XPreLoadControl
	{

		#region " Definition "

		/// <summary>初始動作節點。</summary>
		private class InitialActinoNode
		{

			#region " Properties "

			/// <summary>初始動作。</summary>
			public Action InitialAction;

			/// <summary>動作描述。</summary>
			public string Caption;

			#endregion

			#region " Methods - New "

			/// <summary>建立一個動作清單描述。</summary>
			/// <param name="delAction">初始動作。</param>
			/// <param name="strCaption">動作說明。 </param>
			public InitialActinoNode(Action delAction, string strCaption)
			{
				this.InitialAction = delAction;
				this.Caption = strCaption;
			}

			#endregion

		}

		#endregion

		#region " Properties "

		private XPreLoadViewer g_frmViewer;
		private Action g_delInitial;
		private int g_iNowStep = 1;
		private int g_iStepCount = 1;
		private int g_iInterval;
		private string g_strTittle;

		/// <summary>用來裝動作的容器。</summary>
		private List<InitialActinoNode> g_delActions = new List<InitialActinoNode>();

		#endregion

		#region " Method - New "

		/// <summary>
		/// 建構一個初始控制
		/// </summary>
		/// <param name="delInitial">要初始動作的函式名稱</param>
		public XPreLoadControl(Action delInitial)
		{
			g_delInitial = delInitial;
			g_strTittle = "";
		}

		/// <summary>
		/// 建構一個初始控制
		/// </summary>
		public XPreLoadControl()
		{
			g_delInitial = null;
			g_strTittle = "";
		}

		#endregion

		#region " Methods "

		/// <summary>
		/// 開始執行預載動作。
		/// </summary>
		public void Start()
		{
			//執行 PreLoadUI
			Thread thrPreLoadUI = new Thread(new ThreadStart(PreLoadUI));
			thrPreLoadUI.Name = "PreLoad";
			thrPreLoadUI.Priority = ThreadPriority.Normal;
			thrPreLoadUI.IsBackground = true;
			thrPreLoadUI.Start();

			while (g_frmViewer == null)
			{
				Thread.Sleep(10);
			}

			if (g_delActions.Count == 0)
			{
				// 執行初始化動作
				Thread thrInitial = new Thread(new ThreadStart(g_delInitial));
				thrInitial.Name = "Resource Loader";
				thrInitial.Priority = ThreadPriority.Highest;
				thrInitial.Start();

				// 等待初始化完成
				thrInitial.Join();
			}
			else
			{
				// 設置最大值
				SetStepCount(g_delActions.Count);
				// 執行初始化動作
				Thread thrInitial = new Thread(new ThreadStart(() =>
				{
					for (int iActionIndex = 0; iActionIndex < g_delActions.Count; iActionIndex++)
					{
						NowStatus(g_delActions[iActionIndex].Caption);
                        g_delActions[iActionIndex].InitialAction.Invoke();
					}
					NowStatus("Done!");
					Thread.Sleep(100);
				}));
				thrInitial.Name = "Resource Loader";
				thrInitial.Priority = ThreadPriority.Highest;
				thrInitial.Start();

				// 等待初始化完成
				thrInitial.Join();
			}

			// 關閉預載視窗
			if (g_frmViewer != null)
			{
				g_frmViewer.BeginInvoke(new MethodInvoker(delegate { g_frmViewer.Close(); }));
			}

			thrPreLoadUI.Join();
		}

		private delegate void Update(int iNowStep, string strCaption);
		/// <summary>
		/// 提供控制當前初始進度與描述的方法。
		/// </summary>
		/// <param name="iNowStep">目前進度。</param>
		/// <param name="strCaption">動作描述。</param>
		/// <example>
		/// 使用此方法的話，必須手動給予當前進度。
		/// <code>
		/// private void Example()
		/// {
		///		XPreLoadControl clsPreLoad = new XPreLoadControl(MyFuction);
		///		
		///		clsPreLoad.NowStatus(50,"動作一");
		///		clsPreLoad.NowStatus(100,"動作二");
		/// }
		/// </code>
		/// </example>
		public void NowStatus(int iNowStep, string strCaption)
		{
			if (g_frmViewer.InvokeRequired)
			{
				Update MyUpdate = new Update(NowStatus);
				g_frmViewer.BeginInvoke(MyUpdate, iNowStep, strCaption);
				return;
			}
			else
			{
				g_frmViewer.SetStep(iNowStep);
				g_frmViewer.labStatus.Text = strCaption;
			}
			Thread.Sleep(100);
		}

		/// <summary>
		/// 提供控制當前初始描述的方法，進度部份需搭配 SetStepCount()使用。
		/// <para>進度將會自動前進，每下此行一次便增加一次進度。</para>
		/// </summary>
		/// <param name="strCaption">動作描述。</param>
		/// <example>
		/// 使用方法的話需搭配 SetStepCount()使用。
		/// <code>
		/// private void Example()
		/// {
		///		XPreLoadControl clsPreLoad = new XPreLoadControl(MyFuction);
		///		
		///		clsPreLoad.SetStepCount(2);
		///		clsPreLoad.NowStatus("動作一");
		///		clsPreLoad.NowStatus("動作二");
		/// }
		/// </code>
		/// </example>
		public void NowStatus(string strCaption)
		{
			if (g_iNowStep < g_iStepCount)
			{
				g_frmViewer.BeginInvoke(new MethodInvoker(delegate { g_frmViewer.SetStep(g_iNowStep * g_iInterval); }));
				g_frmViewer.BeginInvoke(new MethodInvoker(delegate { g_frmViewer.labStatus.Text = strCaption; }));
				g_iNowStep++;
			}
			else
			{
				g_frmViewer.BeginInvoke(new MethodInvoker(delegate { g_frmViewer.SetStep(100); }));
				g_frmViewer.BeginInvoke(new MethodInvoker(delegate { g_frmViewer.labStatus.Text = strCaption; }));
				Thread.Sleep(100);
			}

			Thread.Sleep(100);
		}

		/// <summary>
		/// 提供一個設定欲顯示的文字描述，可以是軟體名稱或者版本號。
		/// </summary>
		/// <param name="strTittle">文字描述。</param>
		public void SetTittle(string strTittle)
		{
			g_strTittle = strTittle;
		}

		/// <summary>
		/// 提供一個設定您需要的初始步驟數量，如果有三個初始動作，請設置 3 。
		/// </summary>
		/// <param name="iStep">初始動作數量。</param>
		public void SetStepCount(int iStep)
		{
			g_iStepCount = iStep;

			if (g_iStepCount > 0 || g_iStepCount < 100)
			{
				g_iInterval = 100 / g_iStepCount;
			}
			else
			{
				g_iInterval = 100;
			}

		}

		/// <summary>添加欲初始的動作。</summary>
		/// <param name="delAction">動作名稱。</param>
		/// <param name="strCaption">動作描述。</param>
		public void AddAction(Action delAction, string strCaption)
		{
			g_delActions.Add(new InitialActinoNode(delAction, strCaption));
		}

		/// <summary> 建立預載畫面的UI。</summary>
		private void PreLoadUI()
		{
			g_frmViewer = new XPreLoadViewer();
			g_frmViewer.SetTittle(g_strTittle);
			Application.Run(g_frmViewer);
		}

		#endregion

	}
}
