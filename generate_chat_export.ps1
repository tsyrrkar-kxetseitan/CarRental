# Powershell script to convert transcript.jsonl into a gorgeous, shareable HTML page.
$conversationsDir = "C:\Users\Anton\.gemini\antigravity\brain\2550c060-cb9d-4bc0-a52d-587e6c78e512"
$transcriptPath = Join-Path $conversationsDir ".system_generated\logs\transcript.jsonl"
$outputPath = "c:\Personal\scoala\An3\semestrul2\Industrial Informatics\project\CarRental\RoadRunners_Chat_History.html"

Write-Host "Reading transcript from: $transcriptPath"
if (-not (Test-Path $transcriptPath)) {
    Write-Error "Transcript file not found!"
    exit 1
}

# Read JSONL lines and parse them
$entries = [System.Collections.Generic.List[PSObject]]::new()
$lines = Get-Content -Path $transcriptPath -Raw
# Split by newline and parse each line
$rawLines = $lines -split "`r?`n"
foreach ($line in $rawLines) {
    if ([string]::IsNullOrWhiteSpace($line)) { continue }
    try {
        $obj = ConvertFrom-Json $line
        # Clean up very large items or system-level outputs if needed
        # We want to preserve user inputs and model responses
        if ($obj.type -eq "USER_INPUT" -or $obj.type -eq "PLANNER_RESPONSE") {
            $entries.Add($obj)
        }
    } catch {
        # Skip invalid lines
    }
}

Write-Host "Parsed $($entries.Count) chat messages."

# Serialize to a clean JSON string
$jsonString = ConvertTo-Json $entries -Depth 100

# HTML Template with beautiful premium glassmorphism styling, Marked.js for markdown rendering, and Prism.js for code highlighting.
$htmlContent = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>RoadRunners Car Rental - Development Transcript</title>
    <!-- Google Fonts -->
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;500;600;700;800&family=Plus+Jakarta+Sans:wght@300;400;500;600;700;800&family=Fira+Code:wght@400;500&display=swap" rel="stylesheet">
    
    <!-- Prism.js for syntax highlighting -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/themes/prism-tomorrow.min.css" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/plugins/line-numbers/prism-line-numbers.min.css" />
    
    <!-- FontAwesome for icons -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />

    <style>
        :root {
            --bg-dark: #0b0f19;
            --bg-card: rgba(22, 28, 45, 0.45);
            --border-card: rgba(255, 255, 255, 0.08);
            --primary: #6366f1;
            --primary-glow: rgba(99, 102, 241, 0.15);
            --secondary: #10b981;
            --text-main: #f3f4f6;
            --text-muted: #9ca3af;
            --user-bg: rgba(99, 102, 241, 0.08);
            --user-border: rgba(99, 102, 241, 0.3);
            --assistant-bg: rgba(30, 41, 59, 0.5);
            --assistant-border: rgba(255, 255, 255, 0.08);
        }

        * {
            box-sizing: border-box;
            margin: 0;
            padding: 0;
        }

        body {
            font-family: 'Plus Jakarta Sans', -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
            background-color: var(--bg-dark);
            color: var(--text-main);
            min-height: 100vh;
            line-height: 1.6;
            overflow-x: hidden;
        }

        /* Ambient glow background effects */
        .ambient-glow-1 {
            position: fixed;
            top: -10%;
            left: -10%;
            width: 50%;
            height: 50%;
            background: radial-gradient(circle, rgba(99, 102, 241, 0.08) 0%, rgba(0,0,0,0) 70%);
            z-index: -1;
            pointer-events: none;
        }

        .ambient-glow-2 {
            position: fixed;
            bottom: -10%;
            right: -10%;
            width: 50%;
            height: 50%;
            background: radial-gradient(circle, rgba(16, 185, 129, 0.05) 0%, rgba(0,0,0,0) 70%);
            z-index: -1;
            pointer-events: none;
        }

        .container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 2rem 1.5rem;
        }

        /* Header Styling */
        header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding-bottom: 2rem;
            border-bottom: 1px solid var(--border-card);
            margin-bottom: 3rem;
            flex-wrap: wrap;
            gap: 1.5rem;
        }

        .logo-section {
            display: flex;
            align-items: center;
            gap: 1rem;
        }

        .logo-icon {
            background: linear-gradient(135deg, var(--primary) 0%, #a855f7 100%);
            width: 48px;
            height: 48px;
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 1.5rem;
            box-shadow: 0 8px 24px rgba(99, 102, 241, 0.3);
        }

        .title-group h1 {
            font-family: 'Outfit', sans-serif;
            font-weight: 700;
            font-size: 1.75rem;
            background: linear-gradient(to right, #ffffff, #93c5fd);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
        }

        .title-group p {
            color: var(--text-muted);
            font-size: 0.875rem;
        }

        .meta-badges {
            display: flex;
            gap: 0.75rem;
            flex-wrap: wrap;
        }

        .badge {
            background: rgba(255, 255, 255, 0.05);
            border: 1px solid var(--border-card);
            padding: 0.5rem 1rem;
            border-radius: 9999px;
            font-size: 0.75rem;
            font-weight: 500;
            display: flex;
            align-items: center;
            gap: 0.5rem;
            color: #d1d5db;
        }

        .badge i {
            color: var(--primary);
        }

        .badge.success i {
            color: var(--secondary);
        }

        /* Message Container layout */
        .chat-flow {
            display: flex;
            flex-direction: column;
            gap: 2.5rem;
        }

        .message-row {
            display: flex;
            gap: 1.5rem;
            position: relative;
        }

        .message-avatar {
            width: 44px;
            height: 44px;
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.2rem;
            flex-shrink: 0;
            border: 1px solid rgba(255, 255, 255, 0.1);
        }

        .row-user .message-avatar {
            background: rgba(99, 102, 241, 0.2);
            color: #818cf8;
            border-color: rgba(99, 102, 241, 0.4);
        }

        .row-assistant .message-avatar {
            background: rgba(16, 185, 129, 0.2);
            color: #34d399;
            border-color: rgba(16, 185, 129, 0.4);
        }

        .message-bubble {
            flex-grow: 1;
            background: var(--bg-card);
            border: 1px solid var(--border-card);
            border-radius: 16px;
            padding: 1.5rem 2rem;
            box-shadow: 0 10px 30px -10px rgba(0,0,0,0.3);
            backdrop-filter: blur(12px);
            -webkit-backdrop-filter: blur(12px);
            overflow-x: auto;
            position: relative;
        }

        .row-user .message-bubble {
            background: var(--user-bg);
            border-color: var(--user-border);
            box-shadow: 0 10px 30px -10px rgba(99, 102, 241, 0.1);
        }

        .row-assistant .message-bubble {
            background: var(--assistant-bg);
            border-color: var(--assistant-border);
        }

        .message-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 1rem;
            border-bottom: 1px solid rgba(255, 255, 255, 0.05);
            padding-bottom: 0.75rem;
        }

        .sender-name {
            font-weight: 600;
            font-size: 0.95rem;
            color: #ffffff;
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }

        .msg-timestamp {
            font-size: 0.75rem;
            color: var(--text-muted);
            font-family: 'Outfit', sans-serif;
        }

        /* Markdown Rendering Styling */
        .markdown-body {
            font-size: 0.975rem;
            color: #e5e7eb;
        }

        .markdown-body p {
            margin-bottom: 1rem;
        }

        .markdown-body p:last-child {
            margin-bottom: 0;
        }

        .markdown-body h1, .markdown-body h2, .markdown-body h3, .markdown-body h4 {
            font-family: 'Outfit', sans-serif;
            color: #ffffff;
            margin-top: 1.5rem;
            margin-bottom: 0.75rem;
            font-weight: 600;
        }

        .markdown-body h1 { font-size: 1.4rem; border-bottom: 1px solid rgba(255,255,255,0.1); padding-bottom: 0.3rem; }
        .markdown-body h2 { font-size: 1.25rem; }
        .markdown-body h3 { font-size: 1.1rem; }

        .markdown-body ul, .markdown-body ol {
            margin-left: 1.5rem;
            margin-bottom: 1rem;
        }

        .markdown-body li {
            margin-bottom: 0.25rem;
        }

        .markdown-body code {
            font-family: 'Fira Code', monospace;
            background: rgba(255, 255, 255, 0.08);
            padding: 0.2rem 0.4rem;
            border-radius: 4px;
            font-size: 0.85em;
            color: #f472b6;
        }

        .markdown-body pre code {
            background: none;
            padding: 0;
            color: inherit;
            font-size: inherit;
        }

        .markdown-body pre {
            background: #1e1e2e !important;
            border-radius: 12px;
            padding: 1.25rem;
            margin: 1.2rem 0;
            overflow-x: auto;
            border: 1px solid rgba(255, 255, 255, 0.05);
        }

        .markdown-body blockquote {
            border-left: 4px solid var(--primary);
            background: rgba(99, 102, 241, 0.05);
            padding: 0.75rem 1.25rem;
            margin: 1rem 0;
            border-radius: 0 8px 8px 0;
            color: #d1d5db;
        }

        .markdown-body table {
            width: 100%;
            border-collapse: collapse;
            margin: 1.2rem 0;
        }

        .markdown-body th, .markdown-body td {
            border: 1px solid rgba(255, 255, 255, 0.08);
            padding: 0.75rem 1rem;
            text-align: left;
        }

        .markdown-body th {
            background: rgba(255, 255, 255, 0.04);
            color: #ffffff;
            font-weight: 600;
        }

        .markdown-body tr:nth-child(even) td {
            background: rgba(255, 255, 255, 0.01);
        }

        /* Custom Scrollbar */
        ::-webkit-scrollbar {
            width: 8px;
            height: 8px;
        }

        ::-webkit-scrollbar-track {
            background: var(--bg-dark);
        }

        ::-webkit-scrollbar-thumb {
            background: rgba(255, 255, 255, 0.1);
            border-radius: 999px;
        }

        ::-webkit-scrollbar-thumb:hover {
            background: rgba(255, 255, 255, 0.2);
        }

        /* Floating summary panel */
        .summary-panel {
            background: rgba(30, 41, 59, 0.3);
            border: 1px solid var(--border-card);
            border-radius: 16px;
            padding: 1.25rem;
            margin-bottom: 2.5rem;
            backdrop-filter: blur(12px);
            display: flex;
            align-items: center;
            justify-content: space-between;
            flex-wrap: wrap;
            gap: 1rem;
        }

        .summary-text {
            font-size: 0.9rem;
            color: var(--text-muted);
        }

        .summary-text strong {
            color: white;
        }

        .btn-share {
            background: linear-gradient(135deg, var(--primary) 0%, #4f46e5 100%);
            color: white;
            border: none;
            padding: 0.6rem 1.2rem;
            border-radius: 8px;
            font-weight: 600;
            font-size: 0.85rem;
            cursor: pointer;
            display: flex;
            align-items: center;
            gap: 0.5rem;
            box-shadow: 0 4px 12px rgba(99, 102, 241, 0.2);
            transition: all 0.2s ease;
        }

        .btn-share:hover {
            transform: translateY(-1px);
            box-shadow: 0 6px 16px rgba(99, 102, 241, 0.3);
        }
    </style>
</head>
<body>

    <div class="ambient-glow-1"></div>
    <div class="ambient-glow-2"></div>

    <div class="container">
        <header>
            <div class="logo-section">
                <div class="logo-icon">
                    <i class="fa-solid fa-car-side"></i>
                </div>
                <div class="title-group">
                    <h1>RoadRunners Car Rental</h1>
                    <p>ASP.NET Core MVC & EF Core Development Chat Log</p>
                </div>
            </div>
            
            <div class="meta-badges">
                <div class="badge">
                    <i class="fa-solid fa-user-tie"></i>
                    <span>Student: Mastan Andrei-Mircea</span>
                </div>
                <div class="badge success">
                    <i class="fa-solid fa-circle-check"></i>
                    <span>Status: Scaffolded & Custom Built</span>
                </div>
                <div class="badge">
                    <i class="fa-solid fa-code-branch"></i>
                    <span>Branch: dev</span>
                </div>
            </div>
        </header>

        <div class="summary-panel">
            <div class="summary-text">
                <i class="fa-solid fa-circle-info" style="color: var(--primary); margin-right: 0.5rem;"></i>
                This single-file transcript contains the complete development history. Open in any browser to read. Share via <strong>WhatsApp</strong> or <strong>WeTransfer</strong>!
            </div>
            <button class="btn-share" onclick="window.print()">
                <i class="fa-solid fa-print"></i> Export to PDF
            </button>
        </div>

        <div class="chat-flow" id="chat-flow">
            <!-- Messages rendered dynamically by JavaScript -->
        </div>
    </div>

    <!-- Marked.js for rendering markdown dynamically -->
    <script src="https://cdnjs.cloudflare.com/ajax/libs/marked/4.3.0/marked.min.js"></script>
    <!-- Prism.js for syntax highlighting -->
    <script src="https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/components/prism-core.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/plugins/autoloader/prism-autoloader.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/plugins/line-numbers/prism-line-numbers.min.js"></script>

    <script>
        // Data injected from backend
        const chatData = @jsonString;

        // Custom renderer to enable Prism.js parsing after Markdown is rendered
        marked.setOptions({
            highlight: function(code, lang) {
                if (Prism.languages[lang]) {
                    return Prism.highlight(code, Prism.languages[lang], lang);
                }
                return code;
            },
            breaks: true,
            gfm: true
        });

        const chatFlow = document.getElementById('chat-flow');

        chatData.forEach((msg, idx) => {
            const isUser = msg.source === 'USER_EXPLICIT';
            const row = document.createElement('div');
            row.className = `message-row ` + (isUser ? 'row-user' : 'row-assistant');
            
            const avatar = document.createElement('div');
            avatar.className = 'message-avatar';
            avatar.innerHTML = isUser ? '<i class="fa-solid fa-user"></i>' : '<i class="fa-solid fa-robot"></i>';
            
            const bubble = document.createElement('div');
            bubble.className = 'message-bubble';
            
            const header = document.createElement('div');
            header.className = 'message-header';
            
            const nameSpan = document.createElement('span');
            nameSpan.className = 'sender-name';
            nameSpan.innerHTML = isUser 
                ? '<i class="fa-solid fa-user-circle"></i> Anton / Team Leader' 
                : '<i class="fa-solid fa-circle-nodes"></i> Antigravity AI Coding Companion';
                
            const timeSpan = document.createElement('span');
            timeSpan.className = 'msg-timestamp';
            timeSpan.innerText = new Date(msg.created_at).toLocaleString();
            
            header.appendChild(nameSpan);
            header.appendChild(timeSpan);
            
            const contentDiv = document.createElement('div');
            contentDiv.className = 'markdown-body';
            
            // Clean content from explicit XML tags if needed, or render as is
            let contentText = msg.content || "";
            if (isUser) {
                // Remove raw XML tags for cleaner display
                contentText = contentText.replace(/<USER_REQUEST>/g, '').replace(/<\/USER_REQUEST>/g, '');
            }
            
            contentDiv.innerHTML = marked.parse(contentText);
            
            bubble.appendChild(header);
            bubble.appendChild(contentDiv);
            
            row.appendChild(avatar);
            row.appendChild(bubble);
            
            chatFlow.appendChild(row);
        });

        // Trigger Prism highlight
        setTimeout(() => {
            Prism.highlightAll();
        }, 100);
    </script>
</body>
</html>
"@

# Write output file
$htmlContent | Set-Content -Path $outputPath -Encoding utf8
Write-Host "Success! Generated beautiful HTML transcript at: $outputPath"
