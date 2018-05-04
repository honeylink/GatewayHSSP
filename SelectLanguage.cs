using System;
using System.Collections.Generic;
 
using System.Text;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Windows.Forms;
using System.Threading;
using System.Configuration;
using GatewayHSSP;


namespace GatewayHSSP
{
    public class SelectLanguage
    {
        private string formName;

        public ResourceManager GetCurrentCulture()
        {
            //Thread.CurrentThread.CurrentCulture = 
            //  new System.Globalization.CultureInfo("zh-TW");
            ResourceManager rm = new ResourceManager(
               "GetWaySystems433.Resource1",
               Assembly.GetExecutingAssembly());
            return rm;
        }

        public System.Drawing.Bitmap GetImage(string strObjectId)
        {
            ResourceManager rm = GetCurrentCulture();
            object obj = rm.GetObject(strObjectId);
            return (System.Drawing.Bitmap)obj;
        }

        public string getMsg(string strId)
        {
            string currentLanguage = "";
            try
            {
                ResourceManager rm = GetCurrentCulture();
                string language = ConfigurationManager.AppSettings[Global.Language].ToString();
                System.Globalization.CultureInfo UICulture = new System.Globalization.CultureInfo(language);
                //  Thread.CurrentThread.CurrentUICulture = UICulture;
                CultureInfo ci = UICulture;
                currentLanguage = rm.GetString(strId, ci);
            }
            catch(Exception ex)
            {
                currentLanguage = "";
            }
            return currentLanguage;

        }

        public void SetLanguage(System.Windows.Forms.Control control)
        {
            //MessageBox.Show(control.GetType().BaseType.Name);
            if (control.GetType().BaseType.Name == "Form")
            {
                formName = control.Name;
                control.Text = getMsg(control.Name);
            }

            for (int i = 0; i < control.Controls.Count; i++)
            {
                 if (control.Controls[i].Name.IndexOf("_nlx") > 0)
                        {
                            continue ;
                        }

             
                switch (control.Controls[i].GetType().Name)
                {
                    case "Label":
                    case "Button":
                    case "CheckBox":
                    case "LinkLabel":
                        //MessageBox.Show(formName + control.Controls[i].Name);
                        control.Controls[i].Text = getMsg(
                            formName + control.Controls[i].Name);
                        break;
                    case "Panel":
                    case "GroupBox":
                         control.Controls[i].Text= getMsg(formName + control.Controls[i].Name );
                        SetLanguage(control.Controls[i]);
                        break;
                    case "TabControl":
                   
                        TabControl tbc = (TabControl)control.Controls[i];
                        for (int j = 0; j < tbc.TabCount; j++)
                        {
                            tbc.TabPages[j].Text = getMsg(
                                formName + tbc.TabPages[j].Name);
                            SetLanguage(tbc.TabPages[j]);
                        }
                        break;
                    case "ComboBox":
                       
                         
                            ComboBox cmbox = (ComboBox)control.Controls[i];
                            int l = cmbox.Items.Count;
                            cmbox.Items.Clear();
                            for (int j = 0; j < l; j++)
                            {
                                string item = getMsg(formName + control.Controls[i].Name + "_" + j.ToString());
                                cmbox.Items.Add(item);
                                // cmbox.Items[j] = control.Controls[i].Text = getMsg(formName + control.Controls[i].Name + "_" + j.ToString());
                            }
                       
                        break;
                    case "DataGridView":
                        DataGridView gview = (DataGridView)control.Controls[i];
                        int  len=gview.ColumnCount;
                          for (int j = 0; j < len; j++)
                            {
                                gview.Columns[j].HeaderText = getMsg(formName + control.Controls[i].Name + "_" + j.ToString());
                          }
                     
                        break;
                    default:
                        break;
                }
            }

        }
    }
}
