export interface Exhibit {
  id: number;
  name: string;
  startDate: string;
  endDate: string;
}

export interface ExhibitRevenueLine {
  artworkTitle: string;
  salePrice: number;
}

export interface ExhibitRevenue {
  exhibitId: number;
  total: number;
  lines: ExhibitRevenueLine[];
}
