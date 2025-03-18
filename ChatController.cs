using MetaBIM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Amazon.Runtime.Internal;
using System.Web;
using Newtonsoft.Json;

public class ChatController
{
	//public const string endpoint = "https://api.openai.com/v1/chat/completions";
	//public const string fileEndpoint = "https://api.openai.com/v1/files";
	//public const string assistant = "https://api.openai.com/v1/assistants";
	//public const string thread = "https://api.openai.com/v1/threads";

	public static string UploadFile(string filePath)
	{
		// Ensure the file exists
		if (!File.Exists(filePath))
		{
			return "File not found.";
		}
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Config.fileEndpoint);
		request.Method = "POST";
		request.Headers["Authorization"] = "Bearer " + Config.apiKey;
		string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
		request.ContentType = "multipart/form-data; boundary=" + boundary;

		byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

		using (var requestStream = request.GetRequestStream())
		{
			// purpose part
			string purposePart = "Content-Disposition: form-data; name=\"purpose\"\r\n\r\n" + "assistants";
			byte[] purposeBytes = System.Text.Encoding.UTF8.GetBytes(purposePart);
			requestStream.Write(boundarybytes, 0, boundarybytes.Length);
			requestStream.Write(purposeBytes, 0, purposeBytes.Length);

			// file part
			requestStream.Write(boundarybytes, 0, boundarybytes.Length);
			string headerTemplate = "Content-Disposition: form-data; name=\"file\"; filename=\"{0}\"\r\nContent-Type: application/octet-stream\r\n\r\n";
			string header = string.Format(headerTemplate, Path.GetFileName(filePath));
			byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
			requestStream.Write(headerbytes, 0, headerbytes.Length);

			using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				byte[] buffer = new byte[4096];
				int bytesRead;
				while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
				{
					requestStream.Write(buffer, 0, bytesRead);
				}
			}

			byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
			requestStream.Write(trailer, 0, trailer.Length);
		}

		try
		{
			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
			{
				using (Stream responseStream = response.GetResponseStream())
				{
					using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8))
					{
						string responseJson = reader.ReadToEnd();
						dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseJson);
						return result.id;
					}
				}
			}
		}
		catch (WebException ex)
		{
			return ex.Message;
		}
	}

	public static string CreateAssistant(string fileID)
	{
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Config.assistant);
		request.Method = "POST";
		request.Headers["Authorization"] = "Bearer " + Config.apiKey;
		request.ContentType = "application/json";
		request.Headers["OpenAI-Beta"] = "assistants=v2";
		using (var streamWriter = new StreamWriter(request.GetRequestStream()))
		{
			// Prepare request data
			string requestBody = "{\"model\":\"gpt-4o\",\"tools\":[{\"type\":\"code_interpreter\"}],\"tool_resources\":{\"code_interpreter\":{\"file_ids\":[\"" + fileID + "\"]}}}";
			streamWriter.Write(requestBody);
		}
		try
		{
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			using (Stream responseStream = response.GetResponseStream())
			{
				StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
				string responseJson = reader.ReadToEnd();
				// extract chatgpt's response
				dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseJson);
				//return chatGPTReply;
				return result.id;
			}
		}
		catch (WebException ex)
		{
			// handle exception
			return ex.Message;
		}
	}

	public static string CreateThread()
	{
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Config.thread);
		request.Method = "POST";
		request.Headers["Authorization"] = "Bearer " + Config.apiKey;
		request.ContentType = "application/json";
		request.Headers["OpenAI-Beta"] = "assistants=v2";
		try
		{
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			using (Stream responseStream = response.GetResponseStream())
			{
				StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
				string responseJson = reader.ReadToEnd();
				// extract chatgpt's response
				dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseJson);
				//return chatGPTReply;
				return result.id;
			}
		}
		catch (WebException ex)
		{
			// handle exception
			return ex.Message;
		}
	}

	public static string CreateMessage(string threadID, string fileID, string message)
	{
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		string url = Config.thread + "/" + threadID + "/messages";
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
		request.Method = "POST";
		request.Headers["Authorization"] = "Bearer " + Config.apiKey;
		request.ContentType = "application/json";
		request.Headers["OpenAI-Beta"] = "assistants=v2";
		using (var streamWriter = new StreamWriter(request.GetRequestStream()))
		{
			// Prepare request data
			//string requestBody = "{\"role\":\"user\",\"content\":\"" + message + "\", \"file_ids\":[\"" + fileID + "\"]}";
			//streamWriter.Write(requestBody);
			var data = new
			{
				role = "user",
				content = message,
				attachments = new[]
				{
					new
					{
						file_id = fileID,
						tools = new[]
						{
							new { type = "code_interpreter" }
						}
					}
				}
			};
			string requestBody = JsonConvert.SerializeObject(data);
			streamWriter.Write(requestBody);
		}
		try
		{
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			using (Stream responseStream = response.GetResponseStream())
			{
				StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
				string responseJson = reader.ReadToEnd();
				// extract chatgpt's response
				dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseJson);
				//return chatGPTReply;

				return result.id;
			}
		}
		catch (WebException ex)
		{
			// handle exception
			return ex.Message;
		}
	}

	public static string CreateRuns(string threadID, string assistantID)
	{
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		string url = Config.thread + "/" + threadID + "/runs";
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
		request.Method = "POST";
		request.Headers["Authorization"] = "Bearer " + Config.apiKey;
		request.ContentType = "application/json";
		request.Headers["OpenAI-Beta"] = "assistants=v2";
		using (var streamWriter = new StreamWriter(request.GetRequestStream()))
		{
			// Prepare request data
			string requestBody = "{\"assistant_id\": \"" + assistantID + "\", \"stream\": true}";
			streamWriter.Write(requestBody);
		}
		try
		{
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			using (Stream responseStream = response.GetResponseStream())
			{
				StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
				string responseJson = reader.ReadToEnd();
				return responseJson;
			}
		}
		catch (WebException ex)
		{
			// handle exception
			return ex.Message;
		}
	}

	public static string ListMessage(string threadID)
	{
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		string url = Config.thread + "/" + threadID + "/messages";
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
		request.Method = "GET";
		request.Headers["Authorization"] = "Bearer " + Config.apiKey;
		request.ContentType = "application/json";
		request.Headers["OpenAI-Beta"] = "assistants=v2";
		try
		{
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			using (Stream responseStream = response.GetResponseStream())
			{
				StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
				string responseJson = reader.ReadToEnd();
				return responseJson;
			}
		}
		catch (WebException ex)
		{
			// handle exception
			return ex.Message;
		}
	}

	public static List<string> extractAnswer(string text)
	{
		JObject jsonObject = JObject.Parse(text);
		JArray data = (JArray)jsonObject["data"];
		JToken message = data[0];
		JArray contentArray = (JArray)message["content"];
		JObject contentObject = (JObject)contentArray[0];
		string answer1 = contentObject["text"]["value"].ToString();
		List<string> answer = new List<string>();
		answer.Add(answer1);
		//Regex regex = new Regex(@"(?<json>{(?:[^{}]|(?<open>{)|(?<-open>}))*}(?(open)(?!)))");
		Regex regex = new Regex(@"```(?<content>[\s\S]*?)```|\[(?<bracketsContent>[\s\S]*?)\]");
		Match match = regex.Match(answer1);
		//string answer2 = match.Groups["content"].Value;
		string answer2 = "Match fail";
		if (match.Groups["content"].Success)
		{
			answer2 = match.Groups["content"].Value;
		}
		else if (match.Groups["bracketsContent"].Success)
		{
			answer2 = match.Groups["bracketsContent"].Value;
		}
		answer.Add(answer2);
		return answer;
	}

	public static string getPath(HttpPostedFile file, HttpContext context)
	{
		if (file != null && file.ContentLength > 0)
		{
			string fileName = Path.GetFileName(file.FileName);
			string filePath = context.Server.MapPath(Config.path) + fileName;
			file.SaveAs(filePath);
			return filePath;
		}
		else
		{
			return null;
		}
	}
}