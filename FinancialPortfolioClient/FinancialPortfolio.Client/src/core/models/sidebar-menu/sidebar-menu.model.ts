export interface SidebarMenu {
  id: number;

  title: string;

  icon: string;

  route?: string;

  children?: SidebarMenu[];

  expanded?: boolean;
}
