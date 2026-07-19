export interface CartItem {
  itemId: number;
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface Cart {
  userId: string;
  items: CartItem[];
  totalAmount: number;
}
