# chatgpt-project

## Internship project

Users can upload files and ask questions about this file through openAI's API key. 

Steps:
1. UploadFile  && get a fileID
2. CreateAssistant  && get an assistantID
3. CreateThread  &&  get a threadID
4. CreateMessage using threadID, fileID and the question you want to ask
5. CreateRuns using threadID and assistantID
6. ListMessage using threadID
