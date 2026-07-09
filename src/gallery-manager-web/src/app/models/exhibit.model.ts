export interface Exhibit {
  id: number;
  name: string;
  startDate: string;
  endDate: string;
  artworkCount: number;
  availableCount: number;
  onLoanCount: number;
  soldCount: number;
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
