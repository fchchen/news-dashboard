export interface NewsItemDto {
  id: string;
  externalId: string;
  source: string;
  title: string;
  url: string;
  description: string | null;
  score: number;
  author: string | null;
  company: string;
  tags: string[];
  publishedAt: string;
  metadata: Record<string, string>;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface DashboardSummaryResponse {
  stats: DashboardStats;
  companies: CompanyBreakdown[];
  trendingTopics: TrendingTopicDto[];
  hotItems: NewsItemDto[];
  latestReleases: NewsItemDto[];
  latestBlogPosts: NewsItemDto[];
  lastFetchedAt: string | null;
}

export interface DashboardStats {
  hackerNewsCount: number;
  gitHubReleaseCount: number;
  rssArticleCount: number;
  totalCount: number;
}

export interface CompanyBreakdown {
  company: string;
  itemCount: number;
}

export interface TrendingTopicDto {
  topic: string;
  mentionCount: number;
}

export interface TrendsResponse {
  topics: TrendingTopicDto[];
  companyDistribution: CompanyBreakdown[];
  sourceDistribution: SourceBreakdown[];
}

export interface SourceBreakdown {
  source: string;
  count: number;
}

export interface RssFeedSourceDto {
  name: string;
  url: string;
  company: string;
  filterRequired: boolean;
}
