export type BlogStatus = 'Draft' | 'Published' | 'Closed';

export interface CommentDto {
  id: string;
  userId: string;
  text: string;
  createdAt: string;
}

export interface BlogDto {
  id: string;
  authorId: string;
  title: string;
  description: string;
  createdAt: string;
  images: string[];
  status: BlogStatus;
  comments: CommentDto[];
}

export interface CreateBlogDto {
  title: string;
  description: string;
  imageIds: string[] | null;
}

export interface UploadedImageDto {
  id: string;
  url: string;
}

export interface CreateCommentDto {
  text: string;
}

export interface UpdateCommentDto {
  text: string;
}

export type FollowRequestStatus = 'Pending' | 'Accepted' | 'Rejected';

export interface FollowingDto {
  userId: string;
  username: string | null;
  status: FollowRequestStatus;
}

export interface UserDto {
  userId: string;
  username: string;
}

export interface FollowerDto {
  userId: string;
  username: string | null;
}

export interface FollowersDto {
  pendingRequests: FollowerDto[];
  followers: FollowerDto[];
}

export interface ClubDto {
  id: string;
  ownerId: string;
  name: string;
  description: string;
  images: UploadedImageDto[];
}

export interface CreateClubDto {
  name: string;
  description: string;
  imageIds: string[];
}

export interface UpdateClubDto {
  name: string;
  description: string;
  imageIds: string[];
}
