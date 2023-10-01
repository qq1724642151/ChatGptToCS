using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatGptDemo
{
    public partial class Form1 : Form
    {
        string API_KEY = ConfigurationManager.AppSettings["API_KEY"].Trim(); // 从配置文件中读取 API_KEY
        string Max_Tokens = ConfigurationManager.AppSettings["Max_Tokens"].Trim();//最大文本输出长度
        string Temperature = ConfigurationManager.AppSettings["temperature"].Trim();//文本随机性
   

        string SumPrompts = string.Empty; // 存储用户输入的提示和ChatGPT的回复
        string message = string.Empty; // 存储错误消息

        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_ClickAsync(object sender, EventArgs e)
        {
            string prompts = textBox1.Text.Trim();
            richTextBox1.Text += "发送：" + prompts + "\n"; // 在文本框中显示用户发送的提示
            SumPrompts += prompts + "\n"; // 将用户输入的提示追加到SumPrompts中

            try
            {
                string reply = await GetResponseFromChatGptAsync(); // 调用异步方法获取ChatGPT的回复
                if (!string.IsNullOrEmpty(reply))
                {
                    richTextBox1.Text += "ChatGPT：" + reply + "\n"; // 在文本框中显示ChatGPT的回复
                    SumPrompts += reply + "\n"; // 将ChatGPT的回复追加到SumPrompts中
                }
                else
                {
                    richTextBox1.Text += "报错：" + message; // 在文本框中显示错误消息
                    SumPrompts += reply + "\n"; // 将错误消息追加到SumPrompts中
                }

                textBox1.Text = string.Empty; // 清空用户输入框
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); // 显示异常消息
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 新的 API_KEY 值
            string newApiKey = textBox2.Text.Trim();

            // 更新配置文件中的 API_KEY
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["API_KEY"].Value = newApiKey;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            // 确保在代码中也使用新的 API_KEY 值
            API_KEY = ConfigurationManager.AppSettings["API_KEY"];
            MessageBox.Show("新的API_KEY设置成功，重启程序后生效");
        }

        private async Task<string> GetResponseFromChatGptAsync()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {API_KEY}");

            var jsonContent = new
            {
                model = "text-davinci-003",
                prompt = SumPrompts,
                max_tokens = Convert.ToInt32( Max_Tokens),
                temperature = Convert.ToDouble(Temperature)
            };

            var content = new StringContent(JsonConvert.SerializeObject(jsonContent),
                Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/completions", content);
            string responseString = await response.Content.ReadAsStringAsync();

            return ParseResponseString(responseString); // 调用解析响应字符串的方法
        }

        private string ParseResponseString(string responseString)
        {
            string reply = string.Empty;
            Trace.WriteLine(responseString);

            JArray jArra = JArray.Parse("[" + responseString + "]");
            foreach (var item in jArra)
            {
                Trace.WriteLine(item);
                var str = item["choices"];
                if (item["choices"] != null)
                {
                    foreach (var items in item["choices"])
                    {
                        Trace.WriteLine(item["text"]);
                        reply = items["text"].ToString();
                    }
                }
                else
                {
                    message = item["error"]["message"].ToString();
                }
            }

            return reply; // 返回ChatGPT的回复
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // 新的 API_KEY 值
            string newMax_Tokens = textBox3.Text.Trim();

            // 更新配置文件中的 API_KEY
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["Max_Tokens"].Value = newMax_Tokens;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            // 确保在代码中也使用新的 API_KEY 值
            Max_Tokens =  ConfigurationManager.AppSettings["Max_Tokens"];
            MessageBox.Show("新的Max_Tokens设置成功，重启程序后生效");
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            // 新的 API_KEY 值
            string newTemperature = textBox4.Text.Trim();

            // 更新配置文件中的 API_KEY
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["temperature"].Value = newTemperature;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            // 确保在代码中也使用新的 API_KEY 值
            Temperature =ConfigurationManager.AppSettings["temperature"];
            MessageBox.Show("新的temperature设置成功，重启程序后生效");
        }
    }
}
