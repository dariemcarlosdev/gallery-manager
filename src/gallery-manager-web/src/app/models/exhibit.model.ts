/** Exhibit with rolled-up artwork counts by status. */
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

/** One sold-artwork entry in a revenue breakdown. */
export interface ExhibitRevenueLine {
  artworkTitle: string;
  salePrice: number;
}

/** Revenue summary for an exhibit: total plus per-artwork lines. */
export interface ExhibitRevenue {
  exhibitId: number;
  total: number;
  lines: ExhibitRevenueLine[];
}
