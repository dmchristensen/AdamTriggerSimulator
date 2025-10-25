# Security

Hey! Thanks for checking out the security info. This is a solo project I work on in my free time, so I appreciate you taking the time to read this.

## Found a Security Issue?

If you find something that looks like a security problem, here's what to do:

**For serious security issues:**
- Open a [private security advisory](https://github.com/dmchristensen/AdamTriggerSimulator/security/advisories/new) on GitHub
- This keeps things private until I can fix it

**For less serious stuff:**
- Just open a regular GitHub issue and mention it's security-related
- Or start a discussion if you're not sure

**What to include:**
- What the issue is and how to reproduce it
- Which version you're using
- Any ideas you have for fixing it (totally optional!)

I'll do my best to respond quickly and keep you updated on progress.

## Important Things to Know

### This Tool is for Local Networks Only

The ADAM 6000 devices this app controls don't have encryption or authentication - that's just how they're designed. So please:

- **Only use this on your local, trusted network**
- **Never expose ADAM devices to the internet**
- Think of it like controlling your garage door opener - you wouldn't put that on the internet either!

### Your Data

- Device profiles (IP addresses, names, etc.) are saved as plain text JSON files on your computer
- Location: `%APPDATA%\AdamTriggerSimulator\profiles.json`
- Nothing gets sent anywhere or synced to the cloud
- It's all local to your machine

### What This App Can't Do

- **No encryption** - UDP only through this app
- **No authentication** - Anyone on your network can send commands to ADAM devices (that's how they work)
- **No audit trail** - The log clears when you close the app

These aren't bugs - this is just how ADAM 6000 hardware works. They're designed for use on isolated industrial networks.

## Keeping Secure

Just use common sense:
1. Keep your ADAM devices on an internal network
2. Don't download sketchy device profiles from random places
3. Update to the latest version when you can

## Updates

If there's a security fix, I'll release it as a patch version (like 1.0.1) and you'll see it in the GitHub releases.

---

**Bottom line:** This is a hobby project for controlling industrial I/O modules on local networks. Use it on trusted networks only, and you'll be fine!
