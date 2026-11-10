import { Routes } from '@angular/router';
import { CreateBlog } from './blog-authoring/create-blog/create-blog';
import { MyBlogs } from './blog-authoring/my-blogs/my-blogs';
import { BlogDetail } from './blog-reading/blog-detail/blog-detail';
import { BlogList } from './blog-reading/blog-list/blog-list';
import { FollowerList } from './follower-management/follower-list/follower-list';
import { FollowingList } from './following/following-list/following-list';
import { UserSearch } from './following/user-search/user-search';

export const socialRoutes: Routes = [
  { path: '', component: BlogList },
  { path: 'mine', component: MyBlogs },
  { path: 'create', component: CreateBlog },
  { path: 'users', component: UserSearch },
  { path: 'following', component: FollowingList },
  { path: 'followers', component: FollowerList },
  { path: ':id', component: BlogDetail },
];
