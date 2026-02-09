import { DashboardSummaryResponse, NewsItemDto, PagedResponse, RssFeedSourceDto } from './models/news.models';

const mockHnItems: NewsItemDto[] = [
  {
    id: '1', externalId: 'hn-41234567', source: 'HackerNews',
    title: 'Claude Code now supports MCP hooks and plan mode improvements',
    url: 'https://anthropic.com/news/claude-code-mcp',
    description: 'Anthropic announces major improvements to Claude Code CLI including MCP hook support, enhanced plan mode, and better agentic workflows.',
    score: 542, author: 'anthropic_eng', company: 'Anthropic',
    tags: ['claude', 'claude code', 'mcp', 'agentic'],
    publishedAt: new Date(Date.now() - 2 * 3600000).toISOString(),
    metadata: { commentCount: '187', hnUrl: 'https://news.ycombinator.com/item?id=41234567' }
  },
  {
    id: '2', externalId: 'hn-41234568', source: 'HackerNews',
    title: 'OpenAI releases Codex CLI with full terminal integration',
    url: 'https://openai.com/blog/codex-cli',
    description: 'OpenAI launches Codex CLI, an AI-powered terminal tool that can execute complex development tasks autonomously.',
    score: 398, author: 'openai_dev', company: 'OpenAI',
    tags: ['openai', 'codex', 'ai coding', 'ai cli'],
    publishedAt: new Date(Date.now() - 5 * 3600000).toISOString(),
    metadata: { commentCount: '92', hnUrl: 'https://news.ycombinator.com/item?id=41234568' }
  },
  {
    id: '3', externalId: 'hn-41234569', source: 'HackerNews',
    title: 'Agentic AI workflows are transforming software development',
    url: 'https://example.com/agentic-ai',
    description: 'A deep dive into how agentic AI coding tools like Claude Code, Cursor, and Aider are changing the way developers build software.',
    score: 276, author: 'techwriter', company: 'Other',
    tags: ['agentic', 'ai coding', 'claude code', 'cursor', 'aider'],
    publishedAt: new Date(Date.now() - 8 * 3600000).toISOString(),
    metadata: { commentCount: '54', hnUrl: 'https://news.ycombinator.com/item?id=41234569' }
  },
  {
    id: '4', externalId: 'hn-41234570', source: 'HackerNews',
    title: 'Anthropic Claude 4.5 benchmarks show major coding improvements',
    url: 'https://anthropic.com/news/claude-4-5',
    description: null,
    score: 215, author: 'ml_researcher', company: 'Anthropic',
    tags: ['anthropic', 'claude', 'model release'],
    publishedAt: new Date(Date.now() - 12 * 3600000).toISOString(),
    metadata: { commentCount: '134', hnUrl: 'https://news.ycombinator.com/item?id=41234570' }
  },
  {
    id: '5', externalId: 'hn-41234571', source: 'HackerNews',
    title: 'Cursor vs Claude Code vs Copilot: 2026 AI coding tools comparison',
    url: 'https://example.com/comparison',
    description: 'An in-depth comparison of the leading AI coding assistants, testing them on real-world projects.',
    score: 189, author: 'devtools_fan', company: 'Both',
    tags: ['cursor', 'claude code', 'copilot', 'ai coding'],
    publishedAt: new Date(Date.now() - 18 * 3600000).toISOString(),
    metadata: { commentCount: '67', hnUrl: 'https://news.ycombinator.com/item?id=41234571' }
  }
];

const mockReleases: NewsItemDto[] = [
  {
    id: '10', externalId: 'anthropics/claude-code@v1.0.33', source: 'GitHubRelease',
    title: 'Claude Code v1.0.33',
    url: 'https://github.com/anthropics/claude-code/releases/tag/v1.0.33',
    description: '## What\'s New\n- MCP hook support for pre/post tool execution\n- Improved plan mode with multi-file editing\n- Better context window management\n- Fix: terminal output truncation on large repos',
    score: 0, author: 'anthropics', company: 'Anthropic',
    tags: ['claude-code', 'release', 'cli', 'mcp', 'agentic'],
    publishedAt: new Date(Date.now() - 3 * 3600000).toISOString(),
    metadata: { version: '1.0.33', repoFullName: 'anthropics/claude-code', isPreRelease: 'false' }
  },
  {
    id: '11', externalId: 'openai/codex@v0.1.2', source: 'GitHubRelease',
    title: 'Codex CLI v0.1.2',
    url: 'https://github.com/openai/codex/releases/tag/v0.1.2',
    description: '## Changes\n- Added sandbox mode for safer code execution\n- Multi-turn conversation improvements\n- New --model flag for model selection\n- Bug fixes and performance improvements',
    score: 0, author: 'openai', company: 'OpenAI',
    tags: ['codex', 'release', 'cli', 'openai'],
    publishedAt: new Date(Date.now() - 24 * 3600000).toISOString(),
    metadata: { version: '0.1.2', repoFullName: 'openai/codex', isPreRelease: 'false' }
  },
  {
    id: '12', externalId: 'getcursor/cursor@v0.50.1', source: 'GitHubRelease',
    title: 'Cursor v0.50.1',
    url: 'https://github.com/getcursor/cursor/releases/tag/v0.50.1',
    description: '## Improvements\n- New Background Agent feature for autonomous coding\n- Bug bash: fixed 47 reported issues\n- Improved memory and context handling',
    score: 0, author: 'getcursor', company: 'Other',
    tags: ['cursor', 'release', 'cli', 'ai coding'],
    publishedAt: new Date(Date.now() - 48 * 3600000).toISOString(),
    metadata: { version: '0.50.1', repoFullName: 'getcursor/cursor', isPreRelease: 'false' }
  },
  {
    id: '13', externalId: 'paul-gauthier/aider@v0.82.0', source: 'GitHubRelease',
    title: 'Aider v0.82.0',
    url: 'https://github.com/paul-gauthier/aider/releases/tag/v0.82.0',
    description: '## New Features\n- Claude 4.5 Sonnet support\n- Improved repo map generation\n- Better multi-file editing\n- New /architect command',
    score: 0, author: 'paul-gauthier', company: 'Other',
    tags: ['aider', 'release', 'cli', 'claude'],
    publishedAt: new Date(Date.now() - 72 * 3600000).toISOString(),
    metadata: { version: '0.82.0', repoFullName: 'paul-gauthier/aider', isPreRelease: 'false' }
  },
  {
    id: '14', externalId: 'cline/cline@v3.5.0', source: 'GitHubRelease',
    title: 'Cline v3.5.0',
    url: 'https://github.com/cline/cline/releases/tag/v3.5.0',
    description: '## What\'s Changed\n- New checkpoints feature for undo/redo\n- Model Context Protocol (MCP) integration\n- Enhanced diff view',
    score: 0, author: 'cline', company: 'Other',
    tags: ['cline', 'release', 'cli', 'mcp'],
    publishedAt: new Date(Date.now() - 96 * 3600000).toISOString(),
    metadata: { version: '3.5.0', repoFullName: 'cline/cline', isPreRelease: 'false' }
  },
  {
    id: '15', externalId: 'continuedev/continue@v1.2.0', source: 'GitHubRelease',
    title: 'Continue v1.2.0',
    url: 'https://github.com/continuedev/continue/releases/tag/v1.2.0',
    description: '## Highlights\n- Agent mode for autonomous task completion\n- Improved autocomplete speed\n- New model providers',
    score: 0, author: 'continuedev', company: 'Other',
    tags: ['continue', 'release', 'cli', 'agentic'],
    publishedAt: new Date(Date.now() - 120 * 3600000).toISOString(),
    metadata: { version: '1.2.0', repoFullName: 'continuedev/continue', isPreRelease: 'false' }
  }
];

const mockBlogPosts: NewsItemDto[] = [
  {
    id: '20', externalId: 'rss-abc123', source: 'RssFeed',
    title: 'Introducing Claude 4.5 Sonnet and Haiku',
    url: 'https://anthropic.com/news/claude-4-5',
    description: 'Today we are releasing Claude 4.5 Sonnet and Claude 4.5 Haiku, our latest generation of AI models with significantly improved coding abilities and agentic task completion.',
    score: 0, author: null, company: 'Anthropic',
    tags: ['anthropic', 'claude', 'model release', 'anthropic-blog'],
    publishedAt: new Date(Date.now() - 4 * 3600000).toISOString(),
    metadata: { feedName: 'Anthropic Blog', feedUrl: 'https://www.anthropic.com/news/rss' }
  },
  {
    id: '21', externalId: 'rss-def456', source: 'RssFeed',
    title: 'Codex CLI: AI-powered coding from your terminal',
    url: 'https://openai.com/blog/codex-cli',
    description: 'We are announcing the general availability of Codex CLI, our open-source command-line tool that brings GPT-powered coding assistance directly to your terminal.',
    score: 0, author: null, company: 'OpenAI',
    tags: ['openai', 'codex', 'ai cli', 'ai coding', 'openai-blog'],
    publishedAt: new Date(Date.now() - 10 * 3600000).toISOString(),
    metadata: { feedName: 'OpenAI Blog', feedUrl: 'https://openai.com/blog/rss/' }
  },
  {
    id: '22', externalId: 'rss-ghi789', source: 'RssFeed',
    title: 'The rise of agentic AI: How autonomous coding tools are reshaping development',
    url: 'https://techcrunch.com/agentic-ai-tools',
    description: 'AI coding assistants have evolved from autocomplete to fully autonomous agents. We look at Claude Code, Codex CLI, and how they compare.',
    score: 0, author: 'Sarah Perez', company: 'Both',
    tags: ['agentic', 'ai agent', 'claude code', 'codex', 'techcrunch-ai'],
    publishedAt: new Date(Date.now() - 14 * 3600000).toISOString(),
    metadata: { feedName: 'TechCrunch AI', feedUrl: 'https://techcrunch.com/category/artificial-intelligence/feed/' }
  },
  {
    id: '23', externalId: 'rss-jkl012', source: 'RssFeed',
    title: 'Anthropic raises $5B as AI race heats up',
    url: 'https://www.theverge.com/anthropic-funding',
    description: 'Anthropic has closed a $5 billion funding round, valuing the Claude-maker at $60 billion as competition with OpenAI intensifies.',
    score: 0, author: 'Alex Heath', company: 'Anthropic',
    tags: ['anthropic', 'the-verge-ai'],
    publishedAt: new Date(Date.now() - 20 * 3600000).toISOString(),
    metadata: { feedName: 'The Verge AI', feedUrl: 'https://www.theverge.com/rss/ai-artificial-intelligence/index.xml' }
  }
];

export const mockDashboard: DashboardSummaryResponse = {
  stats: { hackerNewsCount: 24, gitHubReleaseCount: 8, rssArticleCount: 31, totalCount: 63 },
  companies: [
    { company: 'Anthropic', itemCount: 18 },
    { company: 'OpenAI', itemCount: 12 }
  ],
  trendingTopics: [
    { topic: 'claude-code', mentionCount: 14 },
    { topic: 'codex', mentionCount: 9 },
    { topic: 'agentic', mentionCount: 8 },
    { topic: 'mcp', mentionCount: 6 },
    { topic: 'ai-coding', mentionCount: 5 }
  ],
  hotItems: mockHnItems,
  latestReleases: mockReleases.slice(0, 4),
  latestBlogPosts: mockBlogPosts,
  lastFetchedAt: new Date(Date.now() - 2 * 60000).toISOString()
};

export const mockHnResponse: PagedResponse<NewsItemDto> = {
  items: mockHnItems, totalCount: 24, page: 1, pageSize: 20
};

export const mockReleasesResponse: PagedResponse<NewsItemDto> = {
  items: mockReleases, totalCount: 8, page: 1, pageSize: 20
};

export const mockRssResponse: PagedResponse<NewsItemDto> = {
  items: mockBlogPosts, totalCount: 31, page: 1, pageSize: 20
};

export const mockRssSources: RssFeedSourceDto[] = [
  { name: 'Anthropic Blog', url: 'https://www.anthropic.com/news/rss', company: 'Anthropic', filterRequired: false },
  { name: 'OpenAI Blog', url: 'https://openai.com/blog/rss/', company: 'OpenAI', filterRequired: false },
  { name: 'TechCrunch AI', url: 'https://techcrunch.com/category/artificial-intelligence/feed/', company: 'Various', filterRequired: true },
  { name: 'The Verge AI', url: 'https://www.theverge.com/rss/ai-artificial-intelligence/index.xml', company: 'Various', filterRequired: true },
  { name: 'Ars Technica AI', url: 'https://feeds.arstechnica.com/arstechnica/index', company: 'Various', filterRequired: true },
  { name: 'VentureBeat AI', url: 'https://venturebeat.com/category/ai/feed/', company: 'Various', filterRequired: true }
];
