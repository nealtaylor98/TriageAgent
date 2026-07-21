# TriageAgent

A small experimentation with local agents and tooling using microsofts AI Extension. run llama3.2 locally by install ollama on your machine and running `ollama run llama3.2`

once your LLM is running you can start the project with `dotnet run`

use the below cURL command to see it in action. Note that this is just a demo so you wouldn't be able to just whack anything in there


NOTE: Update the port number here to whatever the console displays when you run the dotnet app above. It should say something along the lines of 'Now listening on 5XXX'


<img width="469" height="55" alt="image" src="https://github.com/user-attachments/assets/169fefcd-4cc5-4f62-9bac-b533f329aa05" />

```bash
curl -X POST http://localhost:5117/api/triage \
  -H "Content-Type: application/json" \
  -d '{"systemId": "auth-service", "description": "Users getting 504 Gateway Timeouts during login"}'
```
<img width="833" height="213" alt="image" src="https://github.com/user-attachments/assets/04bc2d80-d6ae-4d15-9790-291017e04bf6" />
