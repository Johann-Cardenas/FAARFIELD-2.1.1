"""
FAARFIELD Documentation Builder
Parses extracted CHM help files and generates a modern single-page documentation site.

Usage:
    python build_docs.py --source C:/tmp/faarfield_help --output Documentation.html
"""

import os
import re
import sys
import base64
import argparse
from html.parser import HTMLParser
from pathlib import Path
from collections import OrderedDict


# ─── TOC Parser ───────────────────────────────────────────────────────────────

class TocEntry:
    def __init__(self, name, local, children=None):
        self.name = name
        self.local = local  # e.g. "Documents/introductiontofaarfield.htm"
        self.children = children or []
        self.slug = self._make_slug(local)

    @staticmethod
    def _make_slug(local):
        if not local:
            return ""
        # Documents/introductiontofaarfield.htm -> introductiontofaarfield
        basename = os.path.basename(local)
        return os.path.splitext(basename)[0]


def parse_hhc(hhc_path):
    """Parse the .hhc sitemap file into a tree of TocEntry objects."""
    with open(hhc_path, "r", encoding="windows-1252", errors="replace") as f:
        content = f.read()

    entries = []
    stack = [entries]
    current_name = None
    current_local = None

    # Simple state machine parser for the HHC format
    i = 0
    while i < len(content):
        # Look for <ul> (push level)
        ul_match = re.match(r'<ul>', content[i:], re.IGNORECASE)
        if ul_match:
            i += ul_match.end()
            # The last entry at the current level gets children
            current_list = stack[-1]
            if current_list:
                new_children = current_list[-1].children
                stack.append(new_children)
            continue

        # Look for </ul> (pop level)
        ul_end = re.match(r'</ul>', content[i:], re.IGNORECASE)
        if ul_end:
            i += ul_end.end()
            if len(stack) > 1:
                stack.pop()
            continue

        # Look for <param name="Name" value="...">
        name_match = re.match(
            r'<param\s+name="Name"\s+value="([^"]*)"',
            content[i:], re.IGNORECASE
        )
        if name_match:
            current_name = name_match.group(1)
            i += name_match.end()
            continue

        # Look for <param name="Local" value="...">
        local_match = re.match(
            r'<param\s+name="Local"\s+value="([^"]*)"',
            content[i:], re.IGNORECASE
        )
        if local_match:
            current_local = local_match.group(1)
            i += local_match.end()
            continue

        # Look for </object> — finalize entry
        obj_end = re.match(r'</object>', content[i:], re.IGNORECASE)
        if obj_end:
            if current_name and current_local:
                entry = TocEntry(current_name, current_local)
                stack[-1].append(entry)
            current_name = None
            current_local = None
            i += obj_end.end()
            continue

        i += 1

    return entries


# ─── Content Extractor ────────────────────────────────────────────────────────

def extract_content(htm_path, source_dir):
    """Extract clean body content from a Doc-To-Help generated HTML file."""
    try:
        with open(htm_path, "r", encoding="windows-1252", errors="replace") as f:
            raw = f.read()
    except FileNotFoundError:
        return "<p><em>Content not available.</em></p>"

    # Extract the nstext div content (main body)
    nstext_match = re.search(
        r'<div\s+id="nstext"[^>]*>(.*?)</div>\s*</body>',
        raw, re.DOTALL | re.IGNORECASE
    )
    if nstext_match:
        body = nstext_match.group(1)
    else:
        # Fallback: try to get body content
        body_match = re.search(r'<body[^>]*>(.*?)</body>', raw, re.DOTALL | re.IGNORECASE)
        body = body_match.group(1) if body_match else raw

    # Remove breadcrumbs div
    body = re.sub(r'<div\s+id="d2h_breadcrumbs"[^>]*>.*?</div>', '', body, flags=re.DOTALL)

    # Remove navigation elements
    body = re.sub(r'<div\s+id="nsbanner"[^>]*>.*?</div>\s*</div>\s*</div>', '', body, flags=re.DOTALL)

    # Remove script tags
    body = re.sub(r'<script[^>]*>.*?</script>', '', body, flags=re.DOTALL)

    # Remove OLE object tags (the cross-reference objects)
    body = re.sub(r'<object\s+id="[^"]*"[^>]*>.*?</object>', '', body, flags=re.DOTALL)

    # Remove related-topics span
    body = re.sub(r'<span\s+id="related-topics"[^>]*>.*?</span>', '', body, flags=re.DOTALL)

    # Extract cross-reference mappings before removing objects
    xref_map = extract_xrefs(raw)

    # Resolve JavaScript cross-references to proper anchor links
    body = resolve_xrefs(body, xref_map)

    # Convert image paths to base64
    body = embed_images(body, source_dir)

    # Clean up MSO-specific styling
    body = clean_mso_html(body)

    return body.strip()


def extract_xrefs(raw_html):
    """Extract TL_xxx -> target URL mappings from OLE object tags."""
    xref_map = {}
    # Pattern: <object id="TL_3" ...><param name="Item1" value=";FAARFIELD.chm::/Documents/xxx.htm" /></object>
    pattern = re.compile(
        r'<object\s+id="((?:TL|RT)_\d+)"[^>]*>.*?'
        r'<param\s+name="Item1"\s+value="[^"]*Documents/([^"#]+?)(?:#[^"]*)?"',
        re.DOTALL | re.IGNORECASE
    )
    for match in pattern.finditer(raw_html):
        obj_id = match.group(1)
        target_file = match.group(2)
        target_slug = os.path.splitext(target_file)[0]
        xref_map[obj_id] = target_slug
    return xref_map


def resolve_xrefs(body, xref_map):
    """Replace JavaScript:TL_xxx.HHClick() links with proper anchor references."""
    def replace_js_link(match):
        obj_id = match.group(1)
        if obj_id in xref_map:
            slug = xref_map[obj_id]
            return f'href="#" data-xref="{slug}" onclick="openReference(\'{slug}\'); return false;"'
        return match.group(0)

    body = re.sub(
        r'href="JavaScript:((?:TL|RT)_\d+)\.HHClick\(\)"',
        replace_js_link, body, flags=re.IGNORECASE
    )

    # Also handle RT_ style links with image buttons (Related Topics)
    def replace_rt_link(match):
        prefix = match.group(1) or ''
        obj_id = match.group(2)
        if obj_id in xref_map:
            slug = xref_map[obj_id]
            return f'{prefix}href="#" data-xref="{slug}" onclick="openReference(\'{slug}\'); return false;"'
        return match.group(0)

    body = re.sub(
        r'(style="[^"]*"\s+)?href="JavaScript:((?:TL|RT)_\d+)\.HHClick\(\)"',
        replace_rt_link, body, flags=re.IGNORECASE
    )

    return body


def embed_images(body, source_dir):
    """Convert image src paths to base64 data URIs."""
    def replace_img(match):
        full_tag = match.group(0)
        src = match.group(1)
        # Resolve relative path
        img_path = os.path.normpath(os.path.join(source_dir, "Documents", src))
        if not os.path.exists(img_path):
            # Try from source root
            img_path = os.path.normpath(os.path.join(source_dir, src.lstrip("../")))
        if not os.path.exists(img_path):
            return full_tag

        try:
            with open(img_path, "rb") as f:
                data = base64.b64encode(f.read()).decode("ascii")
            ext = os.path.splitext(img_path)[1].lower()
            mime = {"png": "image/png", "jpg": "image/jpeg", "jpeg": "image/jpeg",
                    "gif": "image/gif", "bmp": "image/bmp"}.get(ext.lstrip("."), "image/png")
            return full_tag.replace(src, f"data:{mime};base64,{data}")
        except Exception:
            return full_tag

    body = re.sub(r'<img[^>]*\ssrc="([^"]+)"[^>]*>', replace_img, body, flags=re.IGNORECASE)
    return body


def clean_mso_html(body):
    """Clean Microsoft Office-generated HTML artifacts."""
    # Remove anchor name tags that are just bookmarks
    body = re.sub(r'<a\s+name="_[^"]*"\s+id="_[^"]*"\s*>\s*</a>', '', body)

    # Clean up non-breaking spaces (Windows-1252 artifact)
    body = body.replace('\xa0', '&nbsp;')
    body = body.replace('&nbsp;', ' ')
    body = body.replace('\u2019', "'")
    body = body.replace('\u2018', "'")
    body = body.replace('\u201c', '"')
    body = body.replace('\u201d', '"')
    body = body.replace('\u2013', '&ndash;')
    body = body.replace('\u2014', '&mdash;')

    # Remove empty paragraphs
    body = re.sub(r'<p[^>]*>\s*</p>', '', body)

    # Remove excessive inline styles but keep essential ones
    # Keep text-align, font-family (for code), margin-left, text-indent
    def clean_style(match):
        tag_start = match.group(1)
        style = match.group(2)
        tag_end = match.group(3)
        # Keep only useful style properties
        keep_props = []
        for prop in style.split(';'):
            prop = prop.strip()
            if not prop:
                continue
            name = prop.split(':')[0].strip().lower()
            if name in ('text-align', 'font-family', 'margin-left', 'text-indent'):
                keep_props.append(prop)
        if keep_props:
            return f'{tag_start}style="{"; ".join(keep_props)}"{tag_end}'
        return f'{tag_start}{tag_end}'

    body = re.sub(r'(<\w+\s[^>]*)style="([^"]*)"([^>]*>)', clean_style, body)

    # Remove button.gif images (navigation artifacts)
    body = re.sub(r'<img[^>]*src="[^"]*button\.gif"[^>]*/?\s*>', '', body, flags=re.IGNORECASE)

    # Convert class names to semantic ones
    body = body.replace('class="MsoNormal"', 'class="doc-para"')
    body = body.replace('class="MsoNormalTable"', 'class="doc-table"')
    body = body.replace('class="Reference"', 'class="doc-reference"')
    body = body.replace('class="TableText"', 'class="doc-table-text"')
    body = body.replace('class="code-inline"', 'class="doc-code"')
    body = body.replace('class="StyleBold"', 'class="doc-bold"')
    body = body.replace('class="tightspaceCxSpFirst"', 'class="doc-list-item"')
    body = body.replace('class="tightspaceCxSpMiddle"', 'class="doc-list-item"')
    body = body.replace('class="tightspaceCxSpLast"', 'class="doc-list-item"')
    body = body.replace('class="RelatedHead"', 'class="doc-related-head"')

    # Fix Courier New spans to use code styling
    body = re.sub(
        r'<span\s+style="font-family:\s*["\']?Courier New["\']?"[^>]*>(.*?)</span>',
        r'<code class="doc-code">\1</code>',
        body, flags=re.IGNORECASE | re.DOTALL
    )

    return body


# ─── HTML Template ────────────────────────────────────────────────────────────

def get_css():
    return r"""
:root {
    --bg-primary: #fafaf8;
    --bg-sidebar: #1b2a3d;
    --bg-sidebar-hover: #243548;
    --bg-sidebar-active: #2d4259;
    --bg-content: #ffffff;
    --bg-ref-panel: #f7f8fa;
    --bg-code: #f0f2f5;
    --bg-callout: #eef4f3;
    --bg-table-header: #1b2a3d;
    --bg-table-alt: #f5f7fa;
    --border-main: #d4d8dd;
    --border-light: #e8ebef;
    --border-accent: #00796b;
    --text-primary: #1a1f2b;
    --text-secondary: #4a5568;
    --text-sidebar: #c5cdd8;
    --text-sidebar-active: #ffffff;
    --text-muted: #7a8599;
    --text-code: #c7254e;
    --accent-teal: #00796b;
    --accent-teal-light: #e0f2f1;
    --accent-blue: #2e5ea8;
    --accent-amber: #d97706;
    --accent-red: #c53030;
    --font-body: 'Source Serif 4', 'Georgia', 'Cambria', 'Times New Roman', serif;
    --font-heading: 'Inter', 'Segoe UI', system-ui, -apple-system, sans-serif;
    --font-mono: 'JetBrains Mono', 'Cascadia Code', 'Consolas', 'Courier New', monospace;
    --sidebar-w: 300px;
    --ref-panel-w: 42%;
    --header-h: 56px;
    --transition-fast: 0.2s cubic-bezier(0.4, 0, 0.2, 1);
    --transition-medium: 0.35s cubic-bezier(0.4, 0, 0.2, 1);
    --shadow-sm: 0 1px 3px rgba(0,0,0,0.06), 0 1px 2px rgba(0,0,0,0.04);
    --shadow-md: 0 4px 12px rgba(0,0,0,0.08), 0 2px 4px rgba(0,0,0,0.04);
    --shadow-lg: 0 10px 40px rgba(0,0,0,0.12), 0 4px 12px rgba(0,0,0,0.06);
}

@import url('https://fonts.googleapis.com/css2?family=Source+Serif+4:ital,opsz,wght@0,8..60,300..900;1,8..60,300..900&family=Inter:wght@400;500;600;700&family=JetBrains+Mono:wght@400;500&display=swap');

*, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

html { font-size: 16px; scroll-behavior: smooth; height: 100%; }

body {
    font-family: var(--font-body);
    color: var(--text-primary);
    background: var(--bg-primary);
    line-height: 1.72;
    height: 100%;
    overflow: hidden;
    display: flex;
    flex-direction: column;
}

/* ─── Header Bar ──────────────────────────────────────────────────── */

.header {
    height: var(--header-h);
    background: var(--bg-sidebar);
    display: flex;
    align-items: center;
    padding: 0 24px;
    gap: 16px;
    flex-shrink: 0;
    z-index: 100;
    box-shadow: 0 2px 8px rgba(0,0,0,0.15);
}

.header-brand {
    display: flex;
    align-items: baseline;
    gap: 10px;
    flex-shrink: 0;
    cursor: pointer;
    user-select: none;
    padding: 4px 8px;
    margin: -4px -8px;
    border-radius: 6px;
    transition: var(--transition-fast);
}

.header-brand:hover {
    background: rgba(255,255,255,0.06);
}

.header-brand h1 {
    font-family: var(--font-heading);
    font-size: 1.15rem;
    font-weight: 700;
    color: #fff;
    letter-spacing: -0.01em;
}

.header-brand .badge {
    font-family: var(--font-mono);
    font-size: 0.65rem;
    font-weight: 500;
    color: var(--accent-teal);
    background: rgba(0,121,107,0.15);
    padding: 2px 8px;
    border-radius: 10px;
    letter-spacing: 0.03em;
}

.header-search {
    flex: 1;
    max-width: 480px;
    margin: 0 auto;
    position: relative;
}

.header-search input {
    width: 100%;
    padding: 8px 16px 8px 38px;
    border: 1px solid rgba(255,255,255,0.12);
    border-radius: 8px;
    background: rgba(255,255,255,0.07);
    color: #fff;
    font-family: var(--font-heading);
    font-size: 0.85rem;
    outline: none;
    transition: var(--transition-fast);
}

.header-search input::placeholder { color: rgba(255,255,255,0.35); }
.header-search input:focus {
    background: rgba(255,255,255,0.12);
    border-color: var(--accent-teal);
    box-shadow: 0 0 0 3px rgba(0,121,107,0.2);
}

.header-search .search-icon {
    position: absolute;
    left: 12px;
    top: 50%;
    transform: translateY(-50%);
    color: rgba(255,255,255,0.35);
    pointer-events: none;
    font-size: 0.9rem;
}

.search-results {
    position: absolute;
    top: calc(100% + 6px);
    left: 0; right: 0;
    background: var(--bg-content);
    border: 1px solid var(--border-main);
    border-radius: 8px;
    box-shadow: var(--shadow-lg);
    max-height: 360px;
    overflow-y: auto;
    display: none;
    z-index: 200;
}

.search-results.active { display: block; }

.search-result-item {
    padding: 10px 16px;
    cursor: pointer;
    font-family: var(--font-heading);
    font-size: 0.82rem;
    color: var(--text-primary);
    border-bottom: 1px solid var(--border-light);
    transition: var(--transition-fast);
}

.search-result-item:hover { background: var(--accent-teal-light); }
.search-result-item:last-child { border-bottom: none; }

.search-result-item .result-path {
    font-size: 0.72rem;
    color: var(--text-muted);
    margin-top: 2px;
}

.search-result-item mark {
    background: rgba(0,121,107,0.15);
    color: var(--accent-teal);
    padding: 0 2px;
    border-radius: 2px;
}

.header-meta {
    color: rgba(255,255,255,0.4);
    font-family: var(--font-heading);
    font-size: 0.72rem;
    text-align: right;
    flex-shrink: 0;
    line-height: 1.4;
}

/* ─── Layout Shell ────────────────────────────────────────────────── */

.shell {
    display: flex;
    flex: 1;
    overflow: hidden;
}

/* ─── Sidebar ─────────────────────────────────────────────────────── */

.sidebar {
    width: var(--sidebar-w);
    background: var(--bg-sidebar);
    overflow-y: auto;
    flex-shrink: 0;
    border-right: 1px solid rgba(255,255,255,0.06);
    padding: 12px 0;
    scrollbar-width: thin;
    scrollbar-color: rgba(255,255,255,0.12) transparent;
}

.sidebar::-webkit-scrollbar { width: 5px; }
.sidebar::-webkit-scrollbar-track { background: transparent; }
.sidebar::-webkit-scrollbar-thumb { background: rgba(255,255,255,0.12); border-radius: 3px; }

.toc-section-label {
    font-family: var(--font-heading);
    font-size: 0.6rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: rgba(255,255,255,0.25);
    padding: 12px 16px 4px;
}

.toc-item {
    display: flex;
    align-items: baseline;
    padding: 3px 14px 3px calc(14px + var(--indent, 0px));
    font-family: var(--font-heading);
    font-size: 0.76rem;
    font-weight: 400;
    color: var(--text-sidebar);
    text-decoration: none;
    cursor: pointer;
    transition: var(--transition-fast);
    border-left: 2px solid transparent;
    line-height: 1.35;
}

.toc-item:hover {
    background: var(--bg-sidebar-hover);
    color: #e2e8f0;
}

.toc-item.active {
    background: var(--bg-sidebar-active);
    color: var(--text-sidebar-active);
    border-left-color: var(--accent-teal);
    font-weight: 500;
}

.toc-item.depth-1 { --indent: 0px; font-weight: 500; color: #d8dfe8; font-size: 0.77rem; }
.toc-item.depth-2 { --indent: 14px; font-size: 0.74rem; }
.toc-item.depth-3 { --indent: 26px; font-size: 0.72rem; }
.toc-item.depth-4 { --indent: 38px; font-size: 0.7rem; }

/* Separator between top-level groups */
.toc-group > .toc-item.depth-1 { margin-top: 2px; }
.toc-group:first-child > .toc-item.depth-1 { margin-top: 0; }

.toc-toggle {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 14px;
    height: 14px;
    margin-right: 3px;
    font-size: 0.5rem;
    color: rgba(255,255,255,0.3);
    transition: transform 0.2s ease;
    cursor: pointer;
    flex-shrink: 0;
}

.toc-toggle.expanded { transform: rotate(90deg); }

.toc-children { overflow: hidden; }
.toc-children.collapsed { display: none; }

/* ─── Main Content ────────────────────────────────────────────────── */

.content-wrapper {
    flex: 1;
    display: flex;
    overflow: hidden;
    position: relative;
}

.main-content {
    flex: 1;
    overflow-y: auto;
    padding: 0;
    transition: var(--transition-medium);
    scrollbar-width: thin;
    scrollbar-color: rgba(0,0,0,0.12) transparent;
}

.main-content::-webkit-scrollbar { width: 7px; }
.main-content::-webkit-scrollbar-track { background: transparent; }
.main-content::-webkit-scrollbar-thumb { background: rgba(0,0,0,0.12); border-radius: 4px; }

.section-page {
    max-width: 800px;
    margin: 0 auto;
    padding: 40px 48px 80px;
    display: none;
}

.section-page.active { display: block; }

/* ─── Reference Split Panel ───────────────────────────────────────── */

.ref-panel {
    width: 0;
    min-width: 0;
    overflow: hidden;
    background: var(--bg-ref-panel);
    border-left: none;
    transition: width var(--transition-medium), min-width var(--transition-medium), border-left-color 0.15s;
    position: relative;
    flex-shrink: 0;
    display: flex;
    flex-direction: column;
}

.ref-panel.open {
    width: var(--ref-panel-w);
    min-width: 320px;
    border-left: 2px solid var(--border-accent);
}

/* Drag handle on left edge */
.ref-panel-drag {
    position: absolute;
    left: -4px;
    top: 0;
    bottom: 0;
    width: 8px;
    cursor: col-resize;
    z-index: 20;
    opacity: 0;
    transition: opacity 0.15s;
}

.ref-panel.open .ref-panel-drag { opacity: 1; }
.ref-panel-drag:hover, .ref-panel-drag.dragging {
    background: linear-gradient(90deg, transparent 2px, var(--accent-teal) 3px, var(--accent-teal) 5px, transparent 6px);
    opacity: 1;
}

.ref-panel-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 10px 16px;
    background: linear-gradient(135deg, #e0f2f1 0%, #f0faf9 100%);
    border-bottom: 1px solid rgba(0,121,107,0.12);
    flex-shrink: 0;
    gap: 8px;
}

.ref-panel-header h3 {
    font-family: var(--font-heading);
    font-size: 0.82rem;
    font-weight: 600;
    color: var(--accent-teal);
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}

.ref-panel-label {
    font-family: var(--font-heading);
    font-size: 0.58rem;
    text-transform: uppercase;
    letter-spacing: 0.07em;
    color: var(--text-muted);
    margin-bottom: 1px;
}

.ref-panel-actions {
    display: flex;
    align-items: center;
    gap: 6px;
    flex-shrink: 0;
}

.ref-close-btn, .ref-navigate-btn {
    background: none;
    border: 1px solid rgba(0,121,107,0.18);
    border-radius: 5px;
    padding: 4px 10px;
    cursor: pointer;
    font-family: var(--font-heading);
    font-size: 0.7rem;
    font-weight: 500;
    transition: var(--transition-fast);
    white-space: nowrap;
}

.ref-close-btn {
    color: var(--accent-teal);
}

.ref-close-btn:hover {
    background: var(--accent-teal);
    color: #fff;
    border-color: var(--accent-teal);
}

.ref-navigate-btn {
    color: var(--text-muted);
}

.ref-navigate-btn:hover {
    background: var(--bg-sidebar);
    color: #fff;
    border-color: var(--bg-sidebar);
}

.ref-panel-content {
    overflow-y: auto;
    flex: 1;
    padding: 28px 24px 60px;
    scrollbar-width: thin;
    scrollbar-color: rgba(0,0,0,0.08) transparent;
}

.ref-panel-content::-webkit-scrollbar { width: 5px; }
.ref-panel-content::-webkit-scrollbar-thumb { background: rgba(0,0,0,0.1); border-radius: 3px; }

/* Nested xref links inside ref-panel open a tooltip-style hint instead of another panel */
.ref-panel-content a[data-xref] { border-bottom-style: dashed; }
.ref-panel-content a[data-xref]::after { content: ''; }

/* ─── Typography ──────────────────────────────────────────────────── */

.section-page h1 {
    font-family: var(--font-heading);
    font-size: 1.85rem;
    font-weight: 700;
    color: var(--text-primary);
    line-height: 1.25;
    margin-bottom: 8px;
    letter-spacing: -0.02em;
}

.section-page h2 {
    font-family: var(--font-heading);
    font-size: 1.45rem;
    font-weight: 700;
    color: var(--text-primary);
    line-height: 1.3;
    margin: 32px 0 12px;
    letter-spacing: -0.015em;
}

.section-page h3 {
    font-family: var(--font-heading);
    font-size: 1.15rem;
    font-weight: 600;
    color: var(--text-primary);
    line-height: 1.35;
    margin: 28px 0 10px;
}

.section-breadcrumb {
    font-family: var(--font-heading);
    font-size: 0.72rem;
    font-weight: 500;
    color: var(--accent-teal);
    text-transform: uppercase;
    letter-spacing: 0.04em;
    margin-bottom: 12px;
    padding-bottom: 16px;
    border-bottom: 1px solid var(--border-light);
}

.section-divider {
    height: 3px;
    width: 48px;
    background: var(--accent-teal);
    border-radius: 2px;
    margin: 8px 0 28px;
}

.doc-para, .section-page p {
    margin: 0 0 14px;
    text-align: justify;
    hyphens: auto;
    color: #2a2f3a;
}

.doc-reference {
    margin: 0 0 10px;
    padding-left: 28px;
    text-indent: -28px;
    font-size: 0.9rem;
    color: #3d4655;
    line-height: 1.6;
}

.doc-list-item {
    margin: 3px 0;
    padding-left: 28px;
    text-indent: -14px;
    color: #2a2f3a;
}

.doc-related-head {
    font-family: var(--font-heading);
    font-size: 0.85rem;
    font-weight: 600;
    color: var(--accent-teal);
    margin: 24px 0 8px;
    padding-top: 16px;
    border-top: 1px solid var(--border-light);
}

/* ─── Inline Elements ─────────────────────────────────────────────── */

a[data-xref] {
    color: #006259;
    text-decoration: none;
    border-bottom: 1px solid rgba(0,98,89,0.3);
    transition: var(--transition-fast);
    cursor: pointer;
    font-weight: 500;
}

a[data-xref]:hover {
    color: #004d44;
    border-bottom-color: #006259;
    background: rgba(0,121,107,0.06);
    border-radius: 2px;
}

a[data-xref]::after {
    content: '\2197';
    font-size: 0.65em;
    margin-left: 2px;
    opacity: 0.35;
    vertical-align: super;
}

code, .doc-code {
    font-family: var(--font-mono);
    font-size: 0.86em;
    background: #edf0f4;
    padding: 1px 5px;
    border-radius: 3px;
    color: #b83b5e;
    font-weight: 500;
    border: 1px solid rgba(0,0,0,0.04);
}

sub { font-size: 0.75em; }
sup { font-size: 0.75em; }

/* ─── Images ──────────────────────────────────────────────────────── */

.section-page img {
    max-width: 100%;
    height: auto;
    display: block;
    margin: 20px auto;
    border-radius: 3px;
    image-rendering: -webkit-optimize-contrast;
    background: #fff;
}

/* Equation images (small PNGs) get special treatment */
.section-page img[src^="data:"] {
    box-shadow: none;
    border-radius: 0;
    margin: 12px auto;
}

.ref-panel-content img {
    max-width: 100%;
    height: auto;
    display: block;
    margin: 12px auto;
    border-radius: 2px;
    image-rendering: -webkit-optimize-contrast;
}

/* ─── Tables ──────────────────────────────────────────────────────── */

.section-page table, .ref-panel-content table {
    width: 100%;
    border-collapse: collapse;
    margin: 16px 0;
    font-size: 0.9rem;
    border-radius: 6px;
    overflow: hidden;
    box-shadow: var(--shadow-sm);
}

.section-page th, .ref-panel-content th {
    background: var(--bg-table-header);
    color: #fff;
    padding: 10px 14px;
    text-align: left;
    font-family: var(--font-heading);
    font-size: 0.82rem;
    font-weight: 600;
}

.section-page td, .ref-panel-content td {
    padding: 9px 14px;
    border-bottom: 1px solid var(--border-light);
    vertical-align: top;
}

.section-page tr:nth-child(even), .ref-panel-content tr:nth-child(even) {
    background: var(--bg-table-alt);
}

.doc-table-text { margin: 0; }

/* ─── Callout / Note Boxes ────────────────────────────────────────── */

.doc-note {
    border-left: 3px solid var(--accent-teal);
    background: var(--bg-callout);
    padding: 14px 18px;
    margin: 18px 0;
    border-radius: 0 6px 6px 0;
    font-size: 0.92rem;
}

/* ─── Welcome / Landing ───────────────────────────────────────────── */

.landing { text-align: center; padding-top: 80px; }
.landing h1 { font-size: 2.4rem; margin-bottom: 16px; }
.landing .subtitle {
    font-size: 1.05rem;
    color: var(--text-secondary);
    max-width: 520px;
    margin: 0 auto 40px;
}
.landing .quick-links {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 14px;
    max-width: 680px;
    margin: 0 auto;
}
.landing .quick-link {
    padding: 18px;
    border: 1px solid var(--border-light);
    border-radius: 8px;
    cursor: pointer;
    transition: var(--transition-fast);
    text-align: left;
}
.landing .quick-link:hover {
    border-color: var(--accent-teal);
    box-shadow: var(--shadow-md);
    transform: translateY(-1px);
}
.landing .quick-link h3 {
    font-family: var(--font-heading);
    font-size: 0.88rem;
    font-weight: 600;
    color: var(--accent-teal);
    margin: 0 0 4px;
}
.landing .quick-link p {
    font-size: 0.78rem;
    color: var(--text-muted);
    margin: 0;
    text-align: left;
}

/* ─── Keyboard shortcut hint ──────────────────────────────────────── */
kbd {
    display: inline-block;
    padding: 2px 6px;
    font-family: var(--font-mono);
    font-size: 0.75em;
    color: var(--text-secondary);
    background: #f4f5f7;
    border: 1px solid var(--border-main);
    border-radius: 4px;
    box-shadow: 0 1px 1px rgba(0,0,0,0.06);
}

/* ─── Print ───────────────────────────────────────────────────────── */
@media print {
    .header, .sidebar, .ref-panel { display: none; }
    .main-content { overflow: visible; }
    .section-page { display: block !important; max-width: 100%; padding: 20px; page-break-after: always; }
    .section-page h1 { page-break-after: avoid; }
    table { page-break-inside: avoid; }
}

/* ─── Responsive ──────────────────────────────────────────────────── */
@media (max-width: 1100px) {
    :root { --ref-panel-w: 50%; }
}
@media (max-width: 800px) {
    .sidebar { display: none; }
    .section-page { padding: 24px 20px 60px; }
    :root { --ref-panel-w: 100%; }
}
"""


def get_js():
    return r"""
// ─── State ───────────────────────────────────────────────────────────
let currentSection = null;
const sections = {};
const tocIndex = [];

// ─── Initialize ──────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    // Build section lookup
    document.querySelectorAll('.section-page').forEach(el => {
        sections[el.id] = el;
    });

    // Build TOC index for search
    document.querySelectorAll('.toc-item').forEach(el => {
        tocIndex.push({
            name: el.textContent.trim(),
            slug: el.dataset.slug,
            path: el.dataset.path || ''
        });
    });

    // Navigate to hash or show landing
    const hash = window.location.hash.slice(1);
    if (hash && sections[hash]) {
        navigateTo(hash);
    } else {
        showLanding();
    }

    // Search
    const searchInput = document.getElementById('search-input');
    const searchResults = document.getElementById('search-results');
    searchInput.addEventListener('input', () => {
        const q = searchInput.value.trim().toLowerCase();
        if (q.length < 2) {
            searchResults.classList.remove('active');
            return;
        }
        const matches = tocIndex.filter(e =>
            e.name.toLowerCase().includes(q)
        ).slice(0, 12);

        if (matches.length === 0) {
            searchResults.classList.remove('active');
            return;
        }

        searchResults.innerHTML = matches.map(m => {
            const highlighted = m.name.replace(
                new RegExp(`(${q.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')})`, 'gi'),
                '<mark>$1</mark>'
            );
            return `<div class="search-result-item" onclick="navigateTo('${m.slug}'); document.getElementById('search-results').classList.remove('active'); document.getElementById('search-input').value = '';">
                <div>${highlighted}</div>
                ${m.path ? `<div class="result-path">${m.path}</div>` : ''}
            </div>`;
        }).join('');
        searchResults.classList.add('active');
    });

    // Close search on click outside
    document.addEventListener('click', (e) => {
        if (!e.target.closest('.header-search')) {
            searchResults.classList.remove('active');
        }
    });

    // Keyboard: Escape closes ref panel
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') closeRefPanel();
        if (e.key === '/' && !e.ctrlKey && !e.metaKey && document.activeElement.tagName !== 'INPUT') {
            e.preventDefault();
            searchInput.focus();
        }
    });
});

// ─── Navigation ──────────────────────────────────────────────────────
function navigateTo(slug) {
    // Hide all sections
    Object.values(sections).forEach(el => el.classList.remove('active'));

    // Show target section
    const target = sections[slug];
    if (target) {
        target.classList.add('active');
        document.querySelector('.main-content').scrollTop = 0;
    }

    // Update active TOC item
    document.querySelectorAll('.toc-item').forEach(el => {
        el.classList.toggle('active', el.dataset.slug === slug);
    });

    // Ensure parent TOC sections are expanded
    const activeItem = document.querySelector(`.toc-item[data-slug="${slug}"]`);
    if (activeItem) {
        let parent = activeItem.parentElement;
        while (parent) {
            if (parent.classList && parent.classList.contains('toc-children')) {
                parent.classList.remove('collapsed');
                const toggle = parent.previousElementSibling?.querySelector('.toc-toggle');
                if (toggle) toggle.classList.add('expanded');
            }
            parent = parent.parentElement;
        }
        // Scroll TOC to show active item
        activeItem.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
    }

    // Hide landing
    const landing = document.getElementById('landing');
    if (landing) landing.style.display = 'none';

    currentSection = slug;
    window.history.replaceState(null, '', '#' + slug);
}

function showLanding() {
    Object.values(sections).forEach(el => el.classList.remove('active'));
    const landing = document.getElementById('landing');
    if (landing) landing.style.display = 'block';
}

// ─── Home Navigation ─────────────────────────────────────────────────
function goHome() {
    closeRefPanel();
    showLanding();
    document.querySelectorAll('.toc-item').forEach(el => el.classList.remove('active'));
    window.history.replaceState(null, '', window.location.pathname);
}

// ─── Reference Panel (Split Screen) ─────────────────────────────────
function openReference(slug) {
    const panel = document.getElementById('ref-panel');
    const content = document.getElementById('ref-panel-content');
    const title = document.getElementById('ref-panel-title');
    const source = sections[slug];

    if (!source) {
        navigateTo(slug);
        return;
    }

    // Clone content for the reference panel
    content.innerHTML = source.innerHTML;
    title.textContent = source.querySelector('h1, h2, h3')?.textContent || slug;

    // Rewire xref links inside ref-panel to navigate main content (not nest another panel)
    content.querySelectorAll('a[data-xref]').forEach(link => {
        link.onclick = (e) => {
            e.preventDefault();
            const targetSlug = link.dataset.xref;
            // Update the ref-panel content to the new target
            openReference(targetSlug);
        };
    });

    panel.dataset.currentSlug = slug;
    panel.classList.add('open');
}

function closeRefPanel() {
    const panel = document.getElementById('ref-panel');
    panel.classList.remove('open');
}

function navigateToRef() {
    const panel = document.getElementById('ref-panel');
    const slug = panel.dataset.currentSlug;
    closeRefPanel();
    if (slug) navigateTo(slug);
}

// ─── Drag-to-Resize Reference Panel ─────────────────────────────────
(function initDragResize() {
    document.addEventListener('DOMContentLoaded', () => {
        const drag = document.getElementById('ref-panel-drag');
        const panel = document.getElementById('ref-panel');
        const wrapper = document.querySelector('.content-wrapper');
        if (!drag || !panel || !wrapper) return;

        let startX, startW;

        drag.addEventListener('mousedown', (e) => {
            e.preventDefault();
            startX = e.clientX;
            startW = panel.offsetWidth;
            drag.classList.add('dragging');
            panel.style.transition = 'none';

            const onMove = (ev) => {
                const dx = startX - ev.clientX;
                const newW = Math.max(280, Math.min(startW + dx, wrapper.offsetWidth * 0.7));
                panel.style.width = newW + 'px';
            };
            const onUp = () => {
                drag.classList.remove('dragging');
                panel.style.transition = '';
                document.removeEventListener('mousemove', onMove);
                document.removeEventListener('mouseup', onUp);
            };
            document.addEventListener('mousemove', onMove);
            document.addEventListener('mouseup', onUp);
        });
    });
})();

// ─── TOC Toggle ──────────────────────────────────────────────────────
function toggleTocChildren(toggle) {
    const children = toggle.closest('.toc-group')?.querySelector('.toc-children');
    if (children) {
        children.classList.toggle('collapsed');
        toggle.classList.toggle('expanded');
    }
}
"""


# ─── Generator ────────────────────────────────────────────────────────────────

def build_toc_html(entries, depth=1, parent_path=""):
    """Recursively build TOC sidebar HTML."""
    html = ""
    for entry in entries:
        has_children = len(entry.children) > 0
        path = f"{parent_path} > {entry.name}" if parent_path else entry.name

        if has_children:
            html += f'<div class="toc-group">'
            html += f'<a class="toc-item depth-{depth}" data-slug="{entry.slug}" data-path="{parent_path}" onclick="navigateTo(\'{entry.slug}\')">'
            html += f'<span class="toc-toggle" onclick="event.stopPropagation(); toggleTocChildren(this)">&#9654;</span>'
            html += f'{entry.name}</a>\n'
            html += f'<div class="toc-children collapsed">'
            html += build_toc_html(entry.children, min(depth + 1, 4), path)
            html += '</div></div>\n'
        else:
            html += f'<a class="toc-item depth-{depth}" data-slug="{entry.slug}" data-path="{parent_path}" onclick="navigateTo(\'{entry.slug}\')">{entry.name}</a>\n'

    return html


def build_sections_html(entries, source_dir, depth=1):
    """Recursively build section content HTML."""
    html = ""
    for entry in entries:
        htm_path = os.path.join(source_dir, entry.local) if entry.local else None
        content = extract_content(htm_path, source_dir) if htm_path else ""

        # Determine breadcrumb
        crumb_parts = []
        if depth == 1:
            crumb_parts = ["FAARFIELD Documentation"]
        else:
            crumb_parts = ["FAARFIELD Documentation"]

        html += f'<div class="section-page" id="{entry.slug}">\n'
        html += f'  <div class="section-breadcrumb">{" &rsaquo; ".join(crumb_parts)}</div>\n'
        html += f'  {content}\n'
        html += f'  <div class="section-divider" style="margin-top: 32px;"></div>\n'
        html += f'</div>\n'

        if entry.children:
            html += build_sections_html(entry.children, source_dir, depth + 1)

    return html


def build_quick_links(entries):
    """Build landing page quick links from top-level TOC entries."""
    links = []
    descriptions = {
        "Introduction to FAARFIELD": "Overview, background, and program components",
        "Running the Program": "Starting the application, performing designs",
        "Program Interface": "Explorer, toolbar, dock system, navigation",
        "Job and Section Definition and Control": "Creating, saving, and managing jobs and sections",
        "Defining Pavement Structure": "Layer types, materials, editing structures",
        "Defining Section Traffic Mix": "Aircraft library, traffic parameters, user-defined aircraft",
        "Modes of Operation": "Design, Life, Compaction, and PCR modes",
        "Program Output": "Reports, graphs, PDF outputs",
        "Design Options": "Settings, tolerances, advanced options",
        "APPENDIX A: Theoretical Approach to Pavement Design Procedures": "Flexible, rigid, and overlay design theory",
        "APPENDIX B: Cumulative Damage Factor": "CDF concepts and Miner's Rule",
        "APPENDIX C: Modulus Assignment Procedure for Aggregate Layers": "Aggregate modulus computation",
        "APPENDIX D: Data Files": "File management and storage",
        "APPENDIX E: Design Examples": "Step-by-step worked examples",
    }
    for entry in entries:
        desc = descriptions.get(entry.name, "")
        name = entry.name.replace("APPENDIX ", "App. ")
        links.append(
            f'<div class="quick-link" onclick="navigateTo(\'{entry.slug}\')">'
            f'<h3>{name}</h3>'
            f'<p>{desc}</p>'
            f'</div>'
        )
    return '\n'.join(links)


def generate_html(entries, source_dir):
    """Generate the complete documentation HTML page."""
    toc_html = build_toc_html(entries)
    sections_html = build_sections_html(entries, source_dir)
    quick_links = build_quick_links(entries)
    css = get_css()
    js = get_js()

    return f"""<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>FAARFIELD 2.1.1 Documentation</title>
    <style>{css}</style>
</head>
<body>
    <div class="header">
        <div class="header-brand" onclick="goHome()" title="Return to home">
            <h1>FAARFIELD</h1>
            <span class="badge">v2.1.1-CM</span>
        </div>
        <div class="header-search">
            <span class="search-icon">&#128269;</span>
            <input type="text" id="search-input" placeholder="Search documentation...  ( / )" autocomplete="off">
            <div class="search-results" id="search-results"></div>
        </div>
        <div class="header-meta">
            FAA Rigid and Flexible<br>Iterative Elastic Layered Design
        </div>
    </div>

    <div class="shell">
        <nav class="sidebar" id="sidebar">
            <div class="toc-section-label">Documentation</div>
            {toc_html}
        </nav>

        <div class="content-wrapper">
            <main class="main-content" id="main-content">
                <div class="section-page active" id="landing" style="display:block;">
                    <div class="landing">
                        <h1>FAARFIELD Documentation</h1>
                        <div class="section-divider" style="margin: 8px auto 20px;"></div>
                        <p class="subtitle">
                            Airport pavement thickness design and evaluation.
                            Layered elastic theory, 3-D finite element analysis, ACR/PCR classification, and overlay design.
                        </p>
                        <p style="font-size: 0.82rem; color: var(--text-muted); margin-bottom: 32px;">
                            Cross-reference links open in a <strong>split panel</strong> so you can read both sections at once.
                            Press <kbd>Esc</kbd> to close the panel, <kbd>/</kbd> to search.
                        </p>
                        <div class="quick-links">
                            {quick_links}
                        </div>
                    </div>
                </div>
                {sections_html}
            </main>

            <aside class="ref-panel" id="ref-panel">
                <div class="ref-panel-drag" id="ref-panel-drag"></div>
                <div class="ref-panel-header">
                    <div style="min-width:0;">
                        <div class="ref-panel-label">Cross-Reference</div>
                        <h3 id="ref-panel-title"></h3>
                    </div>
                    <div class="ref-panel-actions">
                        <button class="ref-navigate-btn" onclick="navigateToRef()" title="Navigate to this section">Go to section</button>
                        <button class="ref-close-btn" onclick="closeRefPanel()" title="Close panel (Esc)">Close &times;</button>
                    </div>
                </div>
                <div class="ref-panel-content" id="ref-panel-content"></div>
            </aside>
        </div>
    </div>

    <script>{js}</script>
</body>
</html>"""


# ─── Main ─────────────────────────────────────────────────────────────────────

def main():
    parser = argparse.ArgumentParser(description="Build FAARFIELD documentation from extracted CHM.")
    parser.add_argument("--source", default="C:/tmp/faarfield_help",
                        help="Path to extracted CHM directory")
    parser.add_argument("--output", default="Documentation.html",
                        help="Output HTML file path")
    args = parser.parse_args()

    hhc_path = os.path.join(args.source, "FAARFIELD.hhc")
    if not os.path.exists(hhc_path):
        print(f"ERROR: {hhc_path} not found. Extract the CHM first.")
        sys.exit(1)

    print("Parsing TOC structure...")
    entries = parse_hhc(hhc_path)
    print(f"  Found {sum(1 for _ in flatten(entries))} topics in {len(entries)} top-level sections")

    print("Generating documentation HTML...")
    html = generate_html(entries, args.source)

    output_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), args.output)
    with open(output_path, "w", encoding="utf-8") as f:
        f.write(html)

    size_mb = os.path.getsize(output_path) / (1024 * 1024)
    print(f"  Written: {output_path} ({size_mb:.1f} MB)")
    print("Done.")


def flatten(entries):
    """Flatten a tree of TocEntry into an iterator."""
    for e in entries:
        yield e
        yield from flatten(e.children)


if __name__ == "__main__":
    main()
