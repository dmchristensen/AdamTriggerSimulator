# Contributing

Hey! Thanks for your interest in the project. Since this is a hobby project I work on in my spare time, I really appreciate any help or feedback.

## Ways You Can Help

### Found a Bug?

Just open an issue and let me know:
- What happened
- What you expected to happen
- Steps to reproduce it
- Your Windows version and which build you're using (x64, x86, ARM64)
- Screenshots help a lot!

### Have an idea for a feature?

Awesome! Open an issue and tell me about it. I can't promise I'll implement everything (limited free time!), but I love hearing ideas.

### Want to Contribute Code?

That would be amazing! Here's the basic process:

1. **Fork the repo** and make your changes
2. **Test your changes** - make sure it actually works
3. **Submit a pull request** with a description of what you changed and why

Don't worry about making it perfect. We can work together to polish things up.

## Setting Up for Development

### What You Need

- .NET 9.0 SDK or later
- Windows 10/11
- Visual Studio 2022 or VS Code (your choice)

### Getting Started

```bash
# Clone your fork
git clone https://github.com/YOUR-USERNAME/AdamTriggerSimulator.git
cd AdamTriggerSimulator

# Build it
dotnet build

# Run it
dotnet run --project src/AdamTriggerSimulator/AdamTriggerSimulator.csproj

# Run tests
dotnet test
```

### Building Executables Locally

Want to test the single-file executables? Use the PowerShell scripts:

```powershell
.\scripts\build-x64.ps1   # Most common
.\scripts\build-x86.ps1   # 32-bit
.\scripts\build-arm64.ps1 # ARM
```

## Coding Style

I try to keep things clean and consistent:

- Use standard C# naming conventions
- Add comments for complex stuff
- Keep the MVVM pattern (ViewModels, Views, Models, Services)
- If you add public methods, add XML documentation comments

Don't stress too much about it though - I can help clean things up if needed.

## Project Structure

```
src/AdamTriggerSimulator/
├── Models/           # Data structures
├── ViewModels/       # MVVM ViewModels (uses CommunityToolkit.Mvvm)
├── Views/            # XAML UI files
├── Services/         # Business logic and ADAM communication
└── Converters/       # XAML value converters
```

## Commit Messages

Keep them simple:
- "Fix connection timeout issue"
- "Add support for ADAM 6050"
- "Update README with new screenshots"

If it relates to an issue, mention it: "Fixes #42"

## Testing

If you're adding new features, tests are great but not required. I know not everyone has time for that. Just make sure it actually works when you run it!

Existing tests use xUnit and Moq. Run them with `dotnet test`.

## Questions?

Not sure about something? Just ask!
- Open a discussion on GitHub
- Comment on an existing issue
- Or just open a new issue with your question

There are no stupid questions. I'd rather you ask than struggle in silence.

## A Note on Response Time

I work on this in my free time, so I might not respond immediately. I'll try to get back to you within a few days, but sometimes life gets busy. Please be patient!

## License

By contributing, you agree that your contributions will be licensed under the MIT License (same as the project).

---

Thanks for being interested in the project! Even if you don't contribute code, using it and giving feedback is incredibly helpful.
