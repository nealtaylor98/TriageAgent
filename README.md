# TriageAgent

A small experimentation with local agents and tooling using microsofts AI Extension. run llama3.2 locally by install ollama on your machine and running `ollama run llama3.2`

once your LLM is running you can start the project with `dotnet run`

use the below cURL command to see it in action. Note that this is just a demo so you wouldn't be able to just whack anything in there


NOTE: Update the port number here to whatever the console displays when you run the dotnet app above. It should say something along the lines of 'Now listening on 5XXX'


<img width="600" height="115" alt="image" src="https://github.com/user-attachments/assets/6ce3b035-d19e-4f0d-937d-0da2d6d7a42e" />

```bash
curl -X POST http://localhost:5117/api/triage \
  -H "Content-Type: application/json" \
  -d '{"systemId": "auth-service", "description": "Users getting 504 Gateway Timeouts during login"}'
```
