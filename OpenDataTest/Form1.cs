using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace OpenDataTest
{
    public partial class Form1 : Form
    {
        //新北人口: 3986382
        private enum state {debtState,taxState};
        private Point? prePosition = null;
        private ToolTip tooltip = new ToolTip();
        private List<tax> jsonData;
        private int taxhead, taxtail;
        private int chartState;
        public Form1()
        {   
            InitializeComponent();
            prevMonth.Visible = false;
            prevMonth.Enabled = false;
            nextMonth.Visible = false;
            nextMonth.Enabled = false;
            DateLabel.Visible = false;
              
        }

        private void btn(object sender, EventArgs e)
        {
            prevMonth.Visible = false;
            prevMonth.Enabled = false;
            nextMonth.Visible = false;
            nextMonth.Enabled = false;
            chartState = (int)state.debtState;
            //抓取open data
            chart1.Series[0].Points.Clear();
            chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            string url = "http://data.ntpc.gov.tw/od/data/api/94D2B7F1-4AB9-4777-9EB5-9D922112EDD8?$format=json";
            string srcStream = getJsonRequest(url);
            
            //json to object list
            List<debtData> jsonData = JsonConvert.DeserializeObject<List<debtData>>(srcStream);
            //取得第一天時間(年月日)
            DateTime firstDate = getFirstDateFromDebt(jsonData[0].data);
            //繪出平均每人負債圖表
            string[] avgdebt = new string[jsonData.Count];
            for(int i = 0; i < jsonData.Count; i++)
            {
                int index = jsonData[i].field_3.IndexOf('萬');
                string tmp = jsonData[i].field_3.Substring(0, index);
                avgdebt[i] = tmp;
            }
            float[] value = new float[avgdebt.Length];
            for (int i = 0; i < avgdebt.Length; i++)
            {
                value[i] = float.Parse(avgdebt[i]);
            }
            for(int i = 0; i < value.Length; i++)
            {
                chart1.Series[0].Points.AddXY(firstDate.AddMonths(i).ToShortDateString(),value[i]);
            }

            debt.BackColor = Color.FromArgb(80, 80, 80);
            taxState.BackColor = Color.FromArgb(48, 48, 48);

            chart1.Series[0].ChartType = SeriesChartType.Spline;
            chart1.Series[0].Palette = ChartColorPalette.None;
            chart1.ChartAreas[0].AxisX.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chart1.ChartAreas[0].AxisX.MinorGrid.Enabled = true;
            chart1.ChartAreas[0].AxisX.Title = "Date";
            chart1.ChartAreas[0].AxisY.Title = "萬元"; 
            chart1.Titles[0].Text = "人民平均負債";
            //sec chart
            chart2.Series[0].Points.Clear();
            chart2.Series[0].ChartType = SeriesChartType.Spline;
            chart2.Series[0].Palette = ChartColorPalette.None;
            chart2.Titles[0].Text = "人民總負債";
            chart2.ChartAreas[0].AxisY.Title = "億元";
            chart2.ChartAreas[0].AxisX.Title = "Date";

            float[] allDebt = new float[jsonData.Count];
            for(int i = 0; i < jsonData.Count; i++)
            {
                int find = jsonData[i].field_1.IndexOf('億');
                float longTermDebt = float.Parse(jsonData[i].field_1.Substring(0, find));
                find = jsonData[i].field_2.IndexOf('億');
                float shortTermDebt = float.Parse(jsonData[i].field_2.Substring(0, find));
                float result = longTermDebt + shortTermDebt;
                allDebt[i] = result;
            }
            for(int i = 0; i < allDebt.Length; i++)
            {
                chart2.Series[0].Points.AddXY(firstDate.AddMonths(i).ToShortDateString(),allDebt[i]);
            }
            chart2.ChartAreas[1].Visible = false;
            chart2.ChartAreas[0].Position.Width = 94;
            DateLabel.Visible = false;
            
            GC.Collect();
        }

        private void exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void taxState_Click(object sender, EventArgs e)
        {
            chart1.Series[0].Points.Clear();
            chart1.Titles[0].Text = "每月總稅收";
            chart1.ChartAreas[0].AxisX.Title = "Date";
            chart1.ChartAreas[0].AxisY.Title = "元";
            chart2.Series[0].Points.Clear();
            chart2.Series[1].Points.Clear();
            nextMonth.Visible = true;
            nextMonth.Enabled = true;
            prevMonth.Enabled = true;
            prevMonth.Visible = true;
            chartState = (int)state.taxState;
            //各稅收:
            debt.BackColor = Color.FromArgb(48, 48, 48);
            taxState.BackColor = Color.FromArgb(80, 80, 80);
            chart2.ChartAreas[1].Visible = true;
            chart2.ChartAreas[0].Position.Width = 47;
            chart2.Series[0].ChartType = SeriesChartType.Doughnut;
            chart2.Series[0].Palette = ChartColorPalette.Pastel;
            chart2.Titles[0].Text = "每月各別稅收收入";

            string url = "http://data.ntpc.gov.tw/od/data/api/CA74C930-79BE-4A18-AA56-6DAAA0192E39?$format=json";
            string srcStream = getJsonRequest(url);
            srcStream = srcStream.Replace(" ", "");

            jsonData = JsonConvert.DeserializeObject<List<tax>>(srcStream);
            //every month total income tax
            int sum = 0;
            string prev = jsonData[0].Year_month;
            for (int i = 0; i < jsonData.Count; i++)
            {
                if (jsonData[i].Net_collection_moth.Equals("-"))
                {
                    continue;
                }
                else if (jsonData[i].Year_month.Equals(prev))
                {
                    sum += int.Parse(jsonData[i].Net_collection_moth);
                }
                else
                {
                    chart1.Series[0].Points.AddXY(prev, sum);
                    sum = 0;
                    prev = jsonData[i].Year_month;
                }  
            }

            //last month data
            string latestTime = jsonData[jsonData.Count - 1].Year_month;
            taxhead = jsonData.Count - 1;
            for(int i = jsonData.Count-2; i >= 0; i--)
            {
                if (!jsonData[i].Year_month.Equals(latestTime))
                {
                    taxtail = i;
                    break;
                }
                else
                {
                    int taxMoney = int.Parse(jsonData[i].Net_collection_moth);
                    chart2.Series[0].Points.AddXY(jsonData[i].ItemofTax, taxMoney);
                    chart2.Series[1].Points.AddXY(jsonData[i].ItemofTax, taxMoney);
                }
            }
            DateLabel.Visible = true;
            DateLabel.Text = jsonData[taxhead].Year_month;
            GC.Collect();          
        }

        private DateTime getFirstDateFromDebt(string strDate)
        {
            int fYear = strDate.IndexOf('年');
            int fMonth = strDate.IndexOf('月');
            int fDay = strDate.IndexOf('日');

            int year = int.Parse(strDate.Substring(0, fYear));
            year += 1911;
            int month = int.Parse(strDate.Substring(fYear + 1, fMonth - fYear - 1));
            int day = int.Parse(strDate.Substring(fMonth + 1, fDay - fMonth - 1));
            return new DateTime(year, month, day);
        }

        private string getJsonRequest(string url)
        {
            WebRequest request = WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json;charset=UTF-8";
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("UTF-8"));
            string srcStream = reader.ReadToEnd();

            response.Close();
            responseStream.Close();
            reader.Close();
            return srcStream;
        }

        private void chart1MouseMove(object sender, MouseEventArgs e)
        {
            Point pos = e.Location;
            showDataOnChart("Date", "Money", chart1, pos);
        }

        private void chart2MouseMove(object sender, MouseEventArgs e)
        {
            string xName;
            string yName;
            if(chartState == (int)state.taxState)
            {
                xName = "Tax";
                yName = "Money";
            }
            else
            {
                xName = "Date";
                yName = "Money";
            }
            Point pos = e.Location;
            showDataOnChart(xName, yName, chart2, pos);
        }

        private void nextMonth_Click(object sender, EventArgs e)
        {
            if((jsonData.Count-1) == taxhead)
            {
                MessageBox.Show("已是最新月份");
            }
            else
            {
                chart2.Series[1].Points.Clear();
                taxtail = taxhead;
                taxhead += 1;
                string tmp = jsonData[taxhead].Year_month;
                while (jsonData[taxhead].Year_month.Equals(tmp) && taxhead != jsonData.Count - 1)
                {
                    int taxmoney = 0;
                    if (!jsonData[taxhead].Net_collection_moth.Equals("-"))
                        taxmoney = int.Parse(jsonData[taxhead].Net_collection_moth);
                    string taxItem = jsonData[taxhead].ItemofTax;
                    chart2.Series[1].Points.AddXY(taxItem, taxmoney);
                    taxhead++;
                }
                DateLabel.Text = jsonData[taxhead].Year_month;
                if (taxhead != jsonData.Count - 1)
                    taxhead--;
            }
        }

        private void prevMonth_Click(object sender, EventArgs e)
        {
            if (taxtail == 0)
                MessageBox.Show("已是最早月份");
            else
            {
                taxhead = taxtail;
                chart2.Series[1].Points.Clear();
                string tmp = jsonData[taxtail].Year_month;
                while(jsonData[taxtail].Year_month.Equals(tmp) && taxtail != 0)
                {
                    int taxmoney = 0;
                    if (!jsonData[taxtail].Net_collection_moth.Equals("-"))
                        taxmoney = int.Parse(jsonData[taxtail].Net_collection_moth);
                    string taxItem = jsonData[taxtail].ItemofTax;
                    chart2.Series[1].Points.AddXY(taxItem, taxmoney);
                    taxtail--;
                }
                DateLabel.Text = jsonData[taxhead].Year_month;
            }
        }

        private void showDataOnChart(string x, string y, Chart name, Point pos)
        {
            if (prePosition.HasValue && pos == prePosition.Value)
                return;
            tooltip.RemoveAll();
            prePosition = pos;
            var results = name.HitTest(pos.X, pos.Y, false, ChartElementType.DataPoint);
            foreach (var result in results)
            {
                var prop = result.Object as DataPoint;
                if (prop != null)
                {
                    tooltip.Show(x + "=" + prop.AxisLabel + "," + y + "=" + prop.YValues[0], name,
                                        pos.X, pos.Y - 15);
                }
            }
        }
    }
}
