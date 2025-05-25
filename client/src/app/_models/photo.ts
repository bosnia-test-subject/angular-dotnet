import { Tag } from './tag';

export interface Photo {
  id: number;
  url: string;
  isMain: boolean;
  isApproved: boolean;
  username: string;
  PhotoTags?: Tag[];
}
