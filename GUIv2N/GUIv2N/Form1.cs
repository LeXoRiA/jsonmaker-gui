using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUIv2N
{
    public partial class Form1 : Form
    {
        public class FunctionData
        {
            public string screenshotNameObj;
            public string imageURLObj;
            public string destinationImageObj;
            public string templateNameObj;
            public string sourceNameObj;
            public string grayedSourceObj;
            public string cannySourceObj;
            public string resizedCannyObj;
            public string cannyResultObj;
            public string actionObj;
            public string outImageObj;
            public int sleepTimeObj;

            public int startPositionObj;
            public int endPositionObj;

            public string functionName;
            public string methodType;
            
            public bool isValidIR()
            {
                if (String.IsNullOrEmpty(screenshotNameObj) || String.IsNullOrEmpty(imageURLObj) || String.IsNullOrEmpty(destinationImageObj) || String.IsNullOrEmpty(templateNameObj)
                    || String.IsNullOrEmpty(sourceNameObj) || String.IsNullOrEmpty(grayedSourceObj) || String.IsNullOrEmpty(cannySourceObj) || String.IsNullOrEmpty(resizedCannyObj)
                     || String.IsNullOrEmpty(cannyResultObj) || String.IsNullOrEmpty(outImageObj) || String.IsNullOrEmpty(functionName) || String.IsNullOrEmpty(actionObj) || sleepTimeObj == 0)
                {
                    MessageBox.Show("Please make sure you entered all parameters and chose an action. Oh, remember, sleep time cannot be zero!", "Get Low!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                return true;
            } //end isValid()

            public bool isValidLoc()
            {
                if (String.IsNullOrEmpty(functionName) || startPositionObj == 0 || endPositionObj == 0)
                {
                    MessageBox.Show("Please make sure you entered function name and start-end positions!", "Get Low!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                return true;
            } //end isValid()
        } //end FunctionData class
        
        private int startPosition;
        private int endPosition;
        private int buttonClickCount = 0; //set to 0 in constructor
        public string fileName;
        private List<FunctionData> funcParamList = new List<FunctionData>();

        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        }
        
        private void getParams(FunctionData data)
        {
            boxScreenshotNameIR.Text = data.screenshotNameObj;
            boxImageUrlIR.Text = data.imageURLObj;
            boxTemplateNameIR.Text = data.templateNameObj;
            boxOutImageIR.Text = data.outImageObj;
            boxFunctionNameIR.Text = data.functionName;
            sleepTimeIR.Value = data.sleepTimeObj;

            if (data.actionObj == "left")
            {
                radioSwipeLeftIR.PerformClick();
            }
            else if (data.actionObj == "right")
            {
                radioSwipeRightIR.PerformClick();
            }
            else if (data.actionObj == "up")
            {
                radioSwipeUpIR.PerformClick();
            }
            else if (data.actionObj == "down")
            {
                radioSwipeDownIR.PerformClick();
            }
            else if (data.actionObj == "tap")
            {
                radioTapIR.PerformClick();
            }
            return;
        }

        private void btnSaveFile_Click(object sender, EventArgs e)
        {
            
            Dictionary<String, List<FunctionData>> obj = new Dictionary<string, List<FunctionData>>();
            obj.Add("Functions", funcParamList);

            string jsonFunc = JsonConvert.SerializeObject(obj, Formatting.Indented);

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "JSON File|*.json";
            saveFileDialog1.Title = "Save Function File";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                string name = saveFileDialog1.FileName;
                File.WriteAllText(name, jsonFunc);

                statusAction.Text = string.Format("File has been saved as " + name);
                statusTime.Text = string.Format(DateTime.Now.ToString("hh:mm:ss tt"));
                statusStrip1.Refresh();
            }
        }

        private void btnDelFunc_Click(object sender, EventArgs e)
        {
            string message = "Are you sure?";
            string caption = "Delete Function";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            MessageBoxIcon icon = MessageBoxIcon.Warning;
            DialogResult result;
            result = MessageBox.Show(message, caption, buttons, icon);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                if (listBoxfunction.SelectedItem == null)
                {
                    return;
                }
                for (var i = funcParamList.Count - 1; i >= 0; i--)
                {
                    if (funcParamList[i].functionName == listBoxfunction.SelectedItem.ToString())
                    {
                        funcParamList.Remove(funcParamList[i]);
                    }
                } //end for

                listBoxfunction.Items.Remove(listBoxfunction.SelectedItem);
                listBoxfunction.Refresh();

                boxScreenshotNameIR.Text = String.Empty;
                boxImageUrlIR.Text = String.Empty;
                boxTemplateNameIR.Text = String.Empty;
                boxOutImageIR.Text = String.Empty;
                boxFunctionNameIR.Text = String.Empty;
                sleepTimeIR.Value = 0;
                radioSwipeLeftIR.Checked = false;
                radioSwipeRightIR.Checked = false;
                radioSwipeUpIR.Checked = false;
                radioSwipeDownIR.Checked = false;
                radioTapIR.Checked = false;

                startPosition = 0;
                endPosition = 0;

                foreach (Button btns in groupBoxLoc.Controls.OfType<Button>())
                {
                    btns.BackColor = Color.Transparent;
                    buttonClickCount = 0;
                    btns.Enabled = true;
                }
                foreach (TextBox txts in groupBoxLoc.Controls.OfType<TextBox>())
                {
                    txts.Text = String.Empty;
                }
            } //end if

            statusAction.Text = string.Format("Function has been removed!");
            statusTime.Text = string.Format(DateTime.Now.ToString("hh:mm:ss tt"));
            statusStrip1.Refresh();
        }

        private void btnSaveFunc_Click(object sender, EventArgs e)
        {
            FunctionData funcParams = new FunctionData();
            funcParams.screenshotNameObj = boxScreenshotNameIR.Text;
            funcParams.imageURLObj = boxImageUrlIR.Text;
            funcParams.destinationImageObj = boxTemplateNameIR.Text;
            funcParams.templateNameObj = boxTemplateNameIR.Text;
            funcParams.sourceNameObj = boxScreenshotNameIR.Text;
            funcParams.grayedSourceObj = boxScreenshotNameIR.Text + "Grayed";
            funcParams.cannySourceObj = boxScreenshotNameIR.Text + "Canny";
            funcParams.resizedCannyObj = boxScreenshotNameIR.Text + "Resized";
            funcParams.cannyResultObj = boxScreenshotNameIR.Text + "Result";
            funcParams.outImageObj = boxOutImageIR.Text;
            funcParams.sleepTimeObj = Convert.ToInt32(sleepTimeIR.Value);

            funcParams.startPositionObj = startPosition;
            funcParams.endPositionObj = endPosition;

            funcParams.functionName = boxFunctionNameIR.Text;

            if (radioSwipeLeftIR.Checked)
            {
                funcParams.actionObj = "left";
            }
            else if (radioSwipeRightIR.Checked)
            {
                funcParams.actionObj = "right";
            }
            else if (radioSwipeUpIR.Checked)
            {
                funcParams.actionObj = "up";
            }
            else if (radioSwipeDownIR.Checked)
            {
                funcParams.actionObj = "down";
            }
            else if (radioTapIR.Checked)
            {
                funcParams.actionObj = "tap";
            }

            if (funcParams.isValidIR() || funcParams.isValidLoc())
            {
                funcParamList.Add(funcParams);
                listBoxfunction.Items.Equals(boxFunctionNameIR.Text);                
            }

            statusAction.Text = string.Format("Function has been saved!");
            statusTime.Text = string.Format(DateTime.Now.ToString("hh:mm:ss tt"));
            statusStrip1.Refresh();
        }

        private void btnLoadFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog loadFileDialog = new OpenFileDialog();
            loadFileDialog.Title = "Open Function File:";
            loadFileDialog.Filter = "JSON Files|*.json";
            loadFileDialog.InitialDirectory = @"C:\Users\qa1\Desktop\JSONDeneme\";
            loadFileDialog.AddExtension = true;
            loadFileDialog.CheckFileExists = true;

            if (loadFileDialog.ShowDialog() == DialogResult.OK)
            {
                string jsonFileName = loadFileDialog.FileName;
                string jsonFile = File.ReadAllText(jsonFileName);
                var loadedDict = JsonConvert.DeserializeObject<Dictionary<String, List<FunctionData>>>(jsonFile);
                var loadedFile = loadedDict["Functions"];

                funcParamList.Clear();
                listBoxfunction.Items.Clear();

                listBoxfunction.Items.Remove(listBoxfunction.SelectedItem);
                listBoxfunction.Refresh();

                boxScreenshotNameIR.Text = String.Empty;
                boxImageUrlIR.Text = String.Empty;
                boxTemplateNameIR.Text = String.Empty;
                boxOutImageIR.Text = String.Empty;
                boxFunctionNameIR.Text = String.Empty;
                sleepTimeIR.Value = 0;
                radioSwipeLeftIR.Checked = false;
                radioSwipeRightIR.Checked = false;
                radioSwipeUpIR.Checked = false;
                radioSwipeDownIR.Checked = false;
                radioTapIR.Checked = false;

                startPosition = 0;
                endPosition = 0;

                foreach (var obj in loadedFile)
                {
                    if (obj.functionName != null)
                    {
                        getParams(obj);

                        obj.screenshotNameObj = boxScreenshotNameIR.Text;
                        obj.imageURLObj = boxImageUrlIR.Text;
                        obj.destinationImageObj = boxTemplateNameIR.Text;
                        obj.templateNameObj = boxTemplateNameIR.Text;
                        obj.sourceNameObj = boxScreenshotNameIR.Text;
                        obj.grayedSourceObj = boxScreenshotNameIR.Text + "Grayed";
                        obj.cannySourceObj = boxScreenshotNameIR.Text + "Canny";
                        obj.resizedCannyObj = boxScreenshotNameIR.Text + "Resized";
                        obj.cannyResultObj = boxScreenshotNameIR.Text + "Result";
                        obj.outImageObj = boxOutImageIR.Text;
                        obj.sleepTimeObj = Convert.ToInt32(sleepTimeIR.Value);

                        startPosition = obj.startPositionObj;
                        boxStartLoc.Text = startPosition.ToString();
                        endPosition = obj.endPositionObj;
                        boxEndLoc.Text = endPosition.ToString();

                        obj.functionName = boxFunctionNameIR.Text;
                        
                        if (radioSwipeLeftIR.Checked)
                        {
                            obj.actionObj = "left";
                        }
                        else if (radioSwipeRightIR.Checked)
                        {
                            obj.actionObj = "right";
                        }
                        else if (radioSwipeUpIR.Checked)
                        {
                            obj.actionObj = "up";
                        }
                        else if (radioSwipeDownIR.Checked)
                        {
                            obj.actionObj = "down";
                        }
                        else if (radioTapIR.Checked)
                        {
                            obj.actionObj = "tap";
                        }

                      //  if (obj.isValidIR() || obj.isValidLoc())
                      //  {
                            funcParamList.Add(obj);
                            listBoxfunction.Items.Add(obj.functionName);
                        //  }
                    } //end if
                } //end foreach
            } //end if

            statusAction.Text = string.Format("Level has been loaded successfully!");
            statusTime.Text = string.Format(DateTime.Now.ToString("hh:mm:ss tt"));
            statusStrip1.Refresh();
        } //end btnLoadFile_Click

        private void btnClearFuncList_Click(object sender, EventArgs e)
        {
            string message = "All current functions will be deleted. Do you want to continue?";
            string caption = "Clear All Functions";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            MessageBoxIcon icon = MessageBoxIcon.Warning;
            DialogResult result;
            result = MessageBox.Show(message, caption, buttons, icon);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                funcParamList.Clear();
                listBoxfunction.Items.Clear();
            } //end if

            listBoxfunction.Items.Remove(listBoxfunction.SelectedItem);
            listBoxfunction.Refresh();

            boxScreenshotNameIR.Text = String.Empty;
            boxImageUrlIR.Text = String.Empty;
            boxTemplateNameIR.Text = String.Empty;
            boxOutImageIR.Text = String.Empty;
            boxFunctionNameIR.Text = String.Empty;
            sleepTimeIR.Value = 0;
            radioSwipeLeftIR.Checked = false;
            radioSwipeRightIR.Checked = false;
            radioSwipeUpIR.Checked = false;
            radioSwipeDownIR.Checked = false;
            radioTapIR.Checked = false;

            startPosition = 0;
            endPosition = 0;

            foreach (Button btns in groupBoxLoc.Controls.OfType<Button>())
            {
                btns.BackColor = Color.Transparent;
                buttonClickCount = 0;
                btns.Enabled = true;
            }
            foreach (TextBox txts in groupBoxLoc.Controls.OfType<TextBox>())
            {
                txts.Text = String.Empty;
            }

            statusAction.Text = string.Format("All functions have been removed!");
            statusTime.Text = string.Format(DateTime.Now.ToString("hh:mm:ss tt"));
            statusStrip1.Refresh();
        }
        
        private void radioLoc_CheckedChanged(object sender, EventArgs e)
        {
            if (radioLoc.Checked)
            {
                foreach (TextBox txtBox in groupBoxParams.Controls.OfType<TextBox>())
                {
                    txtBox.Enabled = false;
                }

                foreach (RadioButton rdBtn in groupBoxParams.Controls.OfType<RadioButton>())
                {
                    rdBtn.Enabled = false;
                }
                btnClearIR.Enabled = false;
                sleepTimeIR.Enabled = false;
            }
            else
            {
                foreach (TextBox txtBox in groupBoxParams.Controls.OfType<TextBox>())
                {
                    txtBox.Enabled = true;
                }

                foreach (RadioButton rdBtn in groupBoxParams.Controls.OfType<RadioButton>())
                {
                    rdBtn.Enabled = true;
                }
                btnClearIR.Enabled = true;
                sleepTimeIR.Enabled = true;
            }
        }

        private void radioIR_CheckedChanged(object sender, EventArgs e)
        {
            if (radioIR.Checked)
            {

                foreach (Button btn in groupBoxLoc.Controls.OfType<Button>())
                {
                    btn.Enabled = false;
                }
            }

            else
            {
                foreach (Button btn in groupBoxLoc.Controls.OfType<Button>())
                {
                    btn.Enabled = true;
                }
            }
        }

        private void btnClearLoc_Click(object sender, EventArgs e)
        {
            foreach (Button btns in groupBoxLoc.Controls.OfType<Button>())
            {
                btns.BackColor = Color.Transparent;
                buttonClickCount = 0;
                btns.Enabled = true;
            }
            foreach (TextBox txts in groupBoxLoc.Controls.OfType<TextBox>())
            {
                txts.Text = String.Empty;
            }

            statusAction.Text = string.Format("Locations has been cleared!");
            statusTime.Text = string.Format(DateTime.Now.ToString("hh:mm:ss tt"));
            statusStrip1.Refresh();
        }

        private void btnsBoard(object sender, EventArgs e)
        {
            buttonClickCount++; //add 1
            switch (buttonClickCount)
            {
                case 1:
                    //this.btn11.Text = (sender as Button).Text;
                    (sender as Button).BackColor = Color.Green;
                    (sender as Button).Enabled = false;
                    string clickedStart = (sender as Button).Text;
                    startPosition = Convert.ToInt32((sender as Button).Tag);
                    boxStartLoc.Text = (sender as Button).Tag.ToString();
                    //Console.WriteLine(startPosition);
                    break;
                case 2:
                    // this.btn11.Text = (sender as Button).Text;
                    (sender as Button).BackColor = Color.Red;
                    (sender as Button).Enabled = false;
                    foreach (Button btns in groupBoxLoc.Controls.OfType<Button>())
                    {
                        btns.Enabled = false;
                        btnClearLoc.Enabled = true;
                    }
                    string clickedEnd = (sender as Button).Text;
                    endPosition = Convert.ToInt32((sender as Button).Tag);
                    boxEndLoc.Text = (sender as Button).Tag.ToString();
                    //Console.WriteLine(endPosition);
                    break;
                //add other cases here
                default:
                    buttonClickCount--; //add some logic if something unexpected happens
                    break;
            } //end switch
        }

        private void listBoxfunction_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxfunction.SelectedItem == null) { return; }
            foreach (var obj in funcParamList)
            {
                if (obj.functionName == listBoxfunction.SelectedItem.ToString())
                {
                    getParams(obj);        
                }
            } //end for
        }

        private void btnAddFunc_Click(object sender, EventArgs e)
        {
            FunctionData funcParams = new FunctionData();
            funcParams.screenshotNameObj = boxScreenshotNameIR.Text;
            funcParams.imageURLObj = boxImageUrlIR.Text;
            funcParams.destinationImageObj = boxTemplateNameIR.Text;
            funcParams.templateNameObj = boxTemplateNameIR.Text;
            funcParams.sourceNameObj = boxScreenshotNameIR.Text;
            funcParams.grayedSourceObj = boxScreenshotNameIR.Text + "Grayed";
            funcParams.cannySourceObj = boxScreenshotNameIR.Text + "Canny";
            funcParams.resizedCannyObj = boxScreenshotNameIR.Text + "Resized";
            funcParams.cannyResultObj = boxScreenshotNameIR.Text + "Result";
            funcParams.outImageObj = boxOutImageIR.Text;
            funcParams.sleepTimeObj = Convert.ToInt32(sleepTimeIR.Value);

            funcParams.startPositionObj = startPosition;
            funcParams.endPositionObj = endPosition;

            if (radioIR.Checked == true)
            {
                funcParams.methodType = "imageRec";
            }
            else if (radioLoc.Checked == true)
            {
                funcParams.methodType = "location";
            }
            else
            {
                MessageBox.Show("Please select a method", "Invalid Method!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            funcParams.functionName = boxFunctionNameIR.Text;

            if (radioSwipeLeftIR.Checked)
            {
                funcParams.actionObj = "left";
            }
            else if (radioSwipeRightIR.Checked)
            {
                funcParams.actionObj = "right";
            }
            else if (radioSwipeUpIR.Checked)
            {
                funcParams.actionObj = "up";
            }
            else if (radioSwipeDownIR.Checked)
            {
                funcParams.actionObj = "down";
            }
            else if (radioTapIR.Checked)
            {
                funcParams.actionObj = "tap";
            }

            if (radioIR.Checked == true)
            {
                if (funcParams.isValidIR())
                {
                    funcParams.startPositionObj = 0;
                    funcParams.endPositionObj = 0;
                    funcParamList.Add(funcParams);
                    listBoxfunction.Items.Add(boxFunctionNameIR.Text);
                }
            }
            else if (radioLoc.Checked == true)
            {
                if (funcParams.isValidLoc())
                {
                    foreach (TextBox box in groupBoxParams.Controls.OfType<TextBox>())
                    {
                        box.Text = String.Empty;
                    }
                    sleepTimeIR.Value = 0;
                    foreach (RadioButton radio in groupBoxParams.Controls.OfType<RadioButton>())
                    {
                        radio.Checked = false;
                    }
                    funcParamList.Add(funcParams);
                    listBoxfunction.Items.Add(boxFunctionNameIR.Text);                                       
                } //end if
            } //end else if

            statusAction.Text = string.Format("Function has been added!");
            statusTime.Text = string.Format(DateTime.Now.ToString("hh:mm:ss tt"));
            statusStrip1.Refresh();
        } //end btnAddFunc_Click

        private void btnClearIR_Click(object sender, EventArgs e)
        {
            string message = "Are you sure?";
            string caption = "Clear All Parameters";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            MessageBoxIcon icon = MessageBoxIcon.Warning;
            DialogResult result;
            result = MessageBox.Show(message, caption, buttons, icon);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                boxScreenshotNameIR.Text = String.Empty;
                boxImageUrlIR.Text = String.Empty;
                boxTemplateNameIR.Text = String.Empty;
                boxOutImageIR.Text = String.Empty;
                boxFunctionNameIR.Text = String.Empty;
                sleepTimeIR.Value = 0;
                radioSwipeLeftIR.Checked = false;
                radioSwipeRightIR.Checked = false;
                radioSwipeUpIR.Checked = false;
                radioSwipeDownIR.Checked = false;
                radioTapIR.Checked = false;                
            }

            statusAction.Text = string.Format("Paramters has been cleared!");
            statusTime.Text = string.Format(DateTime.Now.ToString("hh:mm:ss tt"));
            statusStrip1.Refresh();
        } //end btnClearIR_Click
    }
}
