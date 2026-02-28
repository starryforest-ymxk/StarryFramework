## Project structure

1. All assets must be placed in the corresponding directories:

- All StarryFramework Code in `/Assets/Plugins/StarryFramework`
- Project information and documentation in `/Assets/Overview`, including:
  - `/Assets/Overview/PROJECT_OVERVIEW.md` (main project overview)
- All test scripts and scenes in `/Assets/Test`
- All plugins in `/Assets/Plugins`

## Coding Guidelines

1. Use self-explanatory names for variables, methods, and types.
2. Add comments for public methods.
3. Use constant fields instead of in-place constants.
4. Keep code simple, readable, and maintainable.
5. Check the **Unity Debug Console** after every code edit to catch errors early.
6. Use **UTF-8 encoding** for Chinese characters. Check the code for garbled characters after every code edit and fix them if necessary.
7. When modifying code, change the code itself **directly** instead of creating a new file to store the modified code.

## Vibe Coding Workflow

1. Before completing a task:
  - Check documentation in `/Assets/Overview` to understand the project
2. For each task:
  - Implement exactly the specified requirements and acceptance criteria.
  - Always provide complete, compilable code snippets for new/modified files.
  - Test changes mentally/simulate Unity behavior.
3. After completing a task:
  - Output a **Task Completion Report** at the end of the response (do not create extra `.md` or readme files).
  - **Synchronously update** `/Assets/Overview/PROJECT_OVERVIEW.md` to reflect new features, file structure, protocols, events, etc.
4. When updating `PROJECT_OVERVIEW.md` :
  - Ensure the documentation matches the current project state precisely.
  - Keep the documentation **concise** and avoid including too much detail.

## Communication Rules

1. Be conversational but concise and professional.
2. Refer to me in the second person and yourself in the first.
3. Respond in the language I used to ask the question.
4. Never say sorry. Instead, try your best to proceed or explain the circumstances to me without apologizing.
